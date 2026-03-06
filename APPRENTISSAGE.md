# Kapoot — Document d’apprentissage (lecture guidée)

Ce document est fait pour **apprendre en lisant ton propre projet**. L’objectif n’est pas seulement de décrire “ce qu’il y a”, mais de t’expliquer **comment ça marche**, pourquoi ça a été conçu comme ça, et de te donner des **exemples concrets** + des **exercices** directement applicables.

> Conseil : lis dans l’ordre. Et à chaque section, ouvre le fichier mentionné dans l’IDE.

---

### 1) Le “modèle mental” de Kapoot

Kapoot, c’est 4 systèmes qui collaborent :

- **Gestion des quiz** (CRUD) : un utilisateur crée un quiz, des questions, des choix, publie.
- **Parties (GameSession)** : un présentateur lance une partie sur un quiz publié → un **code** est généré.
- **Joueurs (Player)** : des joueurs rejoignent la partie avec un **pseudo** (avec ou sans compte).
- **Temps réel (SignalR)** : tout ce qui doit être instantané (afficher question, recevoir réponses, classement, fin de partie).

Le flux “live” ressemble à ça :

1. Host crée une `GameSession` → obtient un **code**.
2. Player rejoint via `POST /api/gamesessions/join` avec `{ code, pseudo }`.
3. Le Player se connecte au hub SignalR `GameHub` et rejoint le **groupe** de la session.
4. Host démarre la partie et “pousse” les questions à tous.
5. Players répondent → API calcule points/classement → renvoie les événements en temps réel.

---

### 2) Architecture backend (Clean Architecture)

Référence : `backend/ARCHITECTURE.md`

Le backend est en “Clean Architecture” :

- **Domain** : le métier “pur” (entités + enums). Aucun EF / aucun ASP.NET ici.
- **Application** : les cas d’usage (commands / queries) + interfaces (`IQuizRepository`, etc.).
- **Infrastructure** : EF Core + repositories concrets + services techniques (hash mdp, etc.).
- **Api** : endpoints HTTP + SignalR hub + wiring (DI, auth, CORS).

Ce que tu dois retenir :

- **Le Domain ne dépend de rien.**
- **Application dépend de Domain.**
- **Infrastructure dépend de Domain + Application (implémente les interfaces).**
- **Api dépend de Application (et s’appuie sur Infrastructure via DI).**

---

### 3) Les données (Domain) : ce qui existe “dans le monde Kapoot”

Quelques entités importantes :

- `Quiz` → contient des `Question`
- `Question` → contient des `Choice` (propositions), peut avoir une explication
- `GameSession` → représente une partie live (code, status, quizId)
- `Player` → représente un joueur dans une session (pseudo, userId optionnel, hasLeft)
- `Answer` → réponse d’un joueur à une question (choix sélectionnés, estCorrect, answeredAt)
- `Score` → total de points + rank

Pourquoi `Player.UserId` est nullable ?

- Parce qu’un joueur peut être **anonyme** : il participe juste avec un **pseudo**.
- Si un utilisateur est connecté, on associe aussi l’ID du compte, mais ce n’est pas obligatoire.

---

### 4) Les endpoints HTTP (Api) — la “porte d’entrée” classique

Fichier principal : `backend/src/Api/Program.cs`

Tu y vois :

- La configuration DB (MySQL en prod si connection string contient `Server=`, sinon SQLite).
- L’enregistrement des repositories + handlers (DI).
- L’auth JWT (et la policy Admin).
- Les routes HTTP (Minimal API).

#### 4.1 Authentification (JWT) + Admin (claims)

Kapoot utilise un **JWT** pour l’authentification.

Idée simple :

- Quand tu te connectes (`/api/auth/login`), l’API génère un token signé.
- Le front stocke ce token et l’envoie sur les appels HTTP via l’en-tête :
  - `Authorization: Bearer <token>`

Pour l’admin :

- Le token inclut un claim `isAdmin=true` si l’utilisateur est admin.
- Une policy `Admin` vérifie ce claim, et protège les endpoints `/api/admin/*`.

À lire dans `backend/src/Api/Program.cs` :

- La policy :
  - `options.AddPolicy("Admin", p => p.RequireClaim("isAdmin", "true"));`
- La génération du token :
  - `if (user.IsAdmin) claims.Add(new Claim("isAdmin", "true"));`
- Les endpoints admin :
  - `.RequireAuthorization("Admin")`

Exemple concret : si tu appelles `GET /api/admin/users` sans être admin → **401/403**.

Exemple concret : **Rejoindre une partie** (c’est la création du `Player`) :

- Front appelle : `POST /api/gamesessions/join`
- Backend exécute le cas d’usage `JoinGameSessionCommandHandler`

Le handler vérifie :

- la session existe et est en `Waiting`,
- le pseudo est obligatoire,
- le pseudo est unique dans la partie,
- si `UserId` est fourni, empêcher “double join” du même compte.

Extrait (à lire dans `backend/src/Application/Commands/JoinGameSession/JoinGameSessionCommandHandler.cs`) :

```csharp
var existingPlayers = await playerRepository.GetByGameSessionIdAsync(session.Id, cancellationToken);
if (existingPlayers.Any(p => p.Pseudo.Equals(command.Pseudo.Trim(), StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("Ce pseudo est déjà pris dans cette partie.");
if (command.UserId is { } uid && existingPlayers.Any(p => p.UserId == uid))
    throw new InvalidOperationException("Vous avez déjà rejoint cette partie.");
```

Résultat : le front reçoit `playerId` + `pseudo` et peut se connecter au hub.

---

### 5) Le temps réel (SignalR) — le cœur du “Kahoot-like”

Fichier : `backend/src/Api/Hubs/GameHub.cs`

Concept clé : **Groupes SignalR**

- Une GameSession correspond à un groupe SignalR : `Session_<sessionId>`
- Tous les clients (host + players) se mettent dans le même groupe
- Le serveur “broadcast” au groupe : *“affiche la question”, “voici le classement”, “partie terminée”, etc.*

#### 5.1 Rejoindre en tant que Host

`JoinAsHost(code)` :

- récupère la session via le code
- met `Context.Items["SessionId"]` et `Context.Items["Role"]="Host"`
- ajoute la connexion au groupe

#### 5.2 Rejoindre en tant que Player

`JoinAsPlayer(code, playerId)` :

- vérifie session + player + cohérence (player appartient à la session)
- stocke `SessionId`, `PlayerId`, `Role="Player"`
- ajoute au groupe

Important : ce design force le player à passer par `POST /api/gamesessions/join` d’abord (création du Player) → puis hub.

#### 5.3 Événements envoyés par le serveur

Les principaux events :

- `ShowQuestion` : envoie la question + choix + `allowMultiple`
- `PlayerAnswered` : juste un feedback “ce joueur a répondu”
- `PointsEarned` : combien de points gagnés + rang d’arrivée (pour feedback UI)
- `Ranking` : classement en temps réel
- `ShowResult` : fin de question (avec explication)
- `GameEnded` : fin de partie + classement final

Exemple : calcul de points par **ordre d’arrivée** (dans `SubmitAnswer`) :

- On compte combien de bonnes réponses existent déjà pour cette question dans la session.
- Le prochain joueur correct obtient un rang (1er, 2e, 3e, …).
- On applique un barème simple (100 / 80 / 60 / 50 / 40).

---

### 6) Frontend (Vue 3 + TS) — comment l’UI consomme l’API

Structure simple :

- `src/router/index.ts` : routes + garde auth/admin
- `src/stores/auth.store.ts` : état utilisateur + token
- `src/services/*` : appels HTTP (API) et SignalR (hub)
- `src/views/*` : écrans (Home, Dashboard, QuizEdit, GameHost, GamePlayer, Join, etc.)

#### 6.1 Comment un joueur rejoint sans compte

Deux chemins :

- **Page publique** `/join` : saisit le code → redirige vers `/game/player/:code`
- **Dans `GamePlayerView`** : si pas de `playerId` dans l’URL, on affiche un formulaire pseudo

Le flow est :

1. `GamePlayerView` appelle `gameSessionService.join({ code, pseudo })`
2. Le backend renvoie `{ playerId, pseudo }`
3. On met `playerId` dans l’URL (query) puis on lance SignalR

À lire :

- `frontend/src/views/JoinView.vue`
- `frontend/src/views/GamePlayerView.vue`
- `frontend/src/services/game-session.service.ts`

#### 6.2 Comment le temps réel arrive côté joueur

Toujours dans `GamePlayerView` :

- On crée une connexion SignalR
- On s’abonne aux events (`ShowQuestion`, `Ranking`, `GameEnded`, etc.)
- On affiche l’UI en fonction de l’état (question courante, attente, fin, classement…)

Tu peux repérer les events côté front en cherchant `conn.on(` dans `GamePlayerView.vue`.

---

### 7) Déploiement (Docker + Caddy)

Référence : `deploy/DEPLOY-M710Q.md`

En production :

- MySQL tourne en conteneur (`kapoot-mysql`)
- L’API tourne en conteneur (`kapoot-api`) et se connecte à MySQL via `Server=mysql`
- Le frontend est servi par Nginx en conteneur (`kapoot-frontend`)
- Caddy (déjà existant sur le serveur) reverse-proxy le sous-domaine vers `kapoot-frontend:80`

Le point le plus important à comprendre :

- **Le frontend est sur le réseau Docker externe `web`**, pour que Caddy puisse l’atteindre.

---

### 8) Exercices (très concrets) pour apprendre vite

Fais-les dans cet ordre, ils sont courts et te feront toucher tout le système.

#### Exercice A — Modifier le barème de points

Objectif : changer la récompense du 1er/2e/3e.

- Backend : dans `GameHub.GetPointsForRank`, change les valeurs.
- Test : démarre une partie, fais répondre 2 joueurs dans l’ordre.
- Bonus : affiche le rang et les points gagnés côté player via l’event `PointsEarned` (déjà en place).

#### Exercice B — Ajouter un “bonus streak” (suite de bonnes réponses)

Objectif : +10 pts si un joueur répond juste 3 fois de suite.

Approche simple :

- Ajoute un champ `CurrentStreak` dans `Score` (Domain + DB).
- Dans `SubmitAnswer` :
  - si correct : `streak++`, sinon `streak=0`
  - si `streak % 3 == 0` : `pointsEarned += 10`
- Front : afficher “Bonus streak +10” dans le toast (tu peux étendre l’event `PointsEarned`).

#### Exercice C — Ajouter un “timer question”

Objectif : le présentateur affiche une question avec durée (ex: 15s), et le serveur refuse les réponses après.

Idée :

- Dans `GameHub.ShowQuestion`, inclure `durationMs`.
- Stocker côté serveur “question ouverte jusqu’à …” (en mémoire, ou en DB si tu veux robuste).
- Dans `SubmitAnswer`, refuser si la question est expirée.
- Front player : afficher un countdown.

#### Exercice D — Faire un petit refactor “propre”

Objectif : réduire la logique dans `GameHub` et déplacer le calcul de points.

- Crée dans `Application` un service `IScoringService`.
- Implémente-le dans `Infrastructure`.
- `GameHub` n’appelle plus `GetPointsForRank` directement, il délègue.

Tu vas comprendre DI + Clean Architecture “en vrai”.

---

### 9) Debug rapide (quand “ça ne marche pas”)

Checklist :

- **Un joueur ne rejoint pas** :
  - regarder la réponse de `POST /api/gamesessions/join` (message d’erreur)
  - vérifier pseudo unique
  - vérifier que la session est `Waiting`
- **SignalR bloqué en “Connexion…”** :
  - vérifier reverse proxy `/hubs/game`
  - vérifier que le `playerId` est bien dans l’URL de connexion
  - regarder les logs du conteneur `kapoot-api`
- **Erreur “module import dynamique” après deploy** :
  - faire un hard refresh (Ctrl+F5)
  - vérifier que `index.html` n’est pas caché (déjà géré côté Nginx)

---

### 10) Par où continuer ton apprentissage ?

Si tu veux progresser vite :

- **SignalR** : lis `GameHub.cs` puis `GamePlayerView.vue` et `GameHostView.vue`
- **Clean Architecture** : lis `JoinGameSessionCommandHandler.cs` puis remonte à `Program.cs` (DI + endpoints)
- **Data/EF** : lis `AppDbContext.cs` puis `Repositories/*`

