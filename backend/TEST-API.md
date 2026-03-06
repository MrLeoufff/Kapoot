# Tests manuels – API KAPOOT

Ce document liste les endpoints du backend pour les tester manuellement (Postman, curl, ou front).

**Base URL** : `https://localhost:5xxx` (remplacer par le port affiché au lancement avec `dotnet run --project backend/src/Api`).

---

## 1. Auth

| Méthode | URL | Body (JSON) | Description |
|--------|-----|-------------|-------------|
| POST | `/api/auth/register` | `{ "email", "password", "pseudo" }` | Inscription. Retourne 201 + user. |
| POST | `/api/auth/login` | `{ "email", "password" }` | Connexion. Retourne 200 + `{ "token", "user" }`. |

---

## 2. Quiz

| Méthode | URL | Body / paramètres | Description |
|--------|-----|-------------------|-------------|
| GET | `/api/quizzes/{id}` | — | Détail d’un quiz (entité brute). |
| GET | `/api/quizzes/{id}/detail` | — | Détail avec questions et choix. |
| GET | `/api/quizzes?ownerId={guid}` | query `ownerId` | Liste des quiz d’un créateur. |
| GET | `/api/quizzes` | sans query | Liste des quiz publiés. |
| GET | `/api/quizzes/published` | — | Liste des quiz publiés. |
| GET | `/api/quizzes/top10` | — | Top 10 des quiz les plus joués. |
| POST | `/api/quizzes` | `{ "title", "description", "ownerId" }` | Créer un quiz (brouillon). |
| PUT | `/api/quizzes/{id}` | `{ "title", "description" }` | Modifier un quiz. |
| POST | `/api/quizzes/{id}/publish` | — | Publier un quiz. |

---

## 3. Questions

| Méthode | URL | Body (JSON) | Description |
|--------|-----|-------------|-------------|
| POST | `/api/quizzes/{quizId}/questions` | `{ "text", "type", "explanation", "order", "choices": [ { "text", "isCorrect", "order" } ] }` | Ajouter une question. `type` : 0 = Mcq, 1 = TrueFalse, 2 = DragDrop. |
| PUT | `/api/questions/{id}` | `{ "text", "explanation", "order", "choices": [ ... ] }` | Modifier une question (remplace les choix). |
| DELETE | `/api/questions/{id}` | — | Supprimer une question. |

---

## 4. Parties (GameSession) et joueurs

| Méthode | URL | Body (JSON) | Description |
|--------|-----|-------------|-------------|
| POST | `/api/gamesessions` | `{ "quizId", "hostId" }` | Créer une partie. Retourne `{ "sessionId", "code" }`. |
| GET | `/api/gamesessions/by-code/{code}` | — | Récupérer une partie par code. |
| POST | `/api/gamesessions/join` | `{ "code", "userId"?, "pseudo" }` | Rejoindre une partie (pseudo obligatoire). |

---

## 5. Profil

| Méthode | URL | Description |
|--------|-----|-------------|
| GET | `/api/users/{userId}/profile` | Profil + stats (nb quiz créés, présentés, parties jouées). |

---

## 6. Temps réel (SignalR)

- **URL du hub** : `https://localhost:5xxx/hubs/game` (en WebSocket).
- Connexion en tant qu’**hôte** : après connexion, appeler `JoinAsHost(code)`.
- Connexion en tant que **joueur** : passer `playerId` en query (pour marquer HasLeft à la déconnexion), puis appeler `JoinAsPlayer(code, playerId)`.
- Méthodes serveur :
  - **Host** : `StartGame`, `ShowQuestion(questionIndex)`, `EndQuestion(questionId, explanation)`, `EndGame`.
  - **Player** : `SubmitAnswer(questionId, choiceIds)`.
- Événements reçus par les clients : `GameStarted`, `ShowQuestion`, `ShowResult`, `Ranking`, `GameEnded`.

---

## Scénario de test rapide

1. **Inscription** : `POST /api/auth/register` avec `email`, `password`, `pseudo` → noter l’`id` utilisateur.
2. **Connexion** : `POST /api/auth/login` → noter le `token` (pour les appels protégés si tu ajoutes l’auth plus tard).
3. **Créer un quiz** : `POST /api/quizzes` avec `title`, `description`, `ownerId` = id utilisateur.
4. **Ajouter une question** : `POST /api/quizzes/{quizId}/questions` avec `text`, `type: 0`, `order: 0`, `choices: [ { "text": "A", "isCorrect": true, "order": 0 }, { "text": "B", "isCorrect": false, "order": 1 } ]`.
5. **Publier le quiz** : `POST /api/quizzes/{quizId}/publish`.
6. **Créer une partie** : `POST /api/gamesessions` avec `quizId`, `hostId` → noter le `code`.
7. **Rejoindre** : `POST /api/gamesessions/join` avec `code`, `pseudo: "Joueur1"`.
8. **Profil** : `GET /api/users/{userId}/profile`.
9. **Top 10** : `GET /api/quizzes/top10`.

Les tests d’intégration (projet `Kapoot.Tests`) couvrent l’auth (register, login). Lancer avec : `dotnet test backend/tests/Kapoot.Tests`.
