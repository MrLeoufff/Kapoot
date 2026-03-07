# Étude de migration du backend Kapoot vers Symfony

**Objectif :** Projet d’étude pour apprenants — analyser la possibilité de migrer le backend actuel (ASP.NET Core) vers Symfony, en conservant une **Clean Architecture**, sans modifier le code existant.

---

## 1. État actuel du backend

### 1.1 Stack technique

| Élément | Actuel (ASP.NET Core 8) |
|--------|--------------------------|
| Langage | C# |
| API HTTP | Minimal API (endpoints dans `Program.cs`) |
| Temps réel | SignalR (hub `/hubs/game`) |
| ORM | Entity Framework Core 8 |
| BDD | SQLite (dev) / MySQL (prod, Pomelo) |
| Auth | JWT (Bearer) |
| Architecture | Clean Architecture + CQRS (commands/queries) |

### 1.2 Couches et rôles

- **Domain** : entités (`User`, `Quiz`, `Question`, `Choice`, `GameSession`, `Player`, `Answer`, `Score`), enums (`GameSessionStatus`, `QuestionType`, `QuizStatus`). Aucune dépendance externe.
- **Application** : interfaces (repositories, `IPasswordHasher`), handlers CQRS (CreateQuiz, JoinGameSession, Login, etc.), DTOs. Dépend uniquement de Domain.
- **Infrastructure** : implémentations des repositories (EF Core), `AppDbContext`, `PasswordHasher` (BCrypt). Dépend de Domain + Application.
- **Api** : configuration (DI, JWT, CORS, SignalR), routes HTTP, `GameHub` SignalR. Dépend de Application (+ Infrastructure via DI).

### 1.3 Périmètre à reproduire

- **~25+ routes HTTP** (auth, quizzes, questions, game sessions, users, admin).
- **1 hub SignalR** avec méthodes : `JoinAsHost`, `JoinAsPlayer`, `StartGame`, `ShowQuestion`, `SubmitAnswer`, `EndQuestion`, `EndGame` + événements temps réel (`GameStarted`, `ShowQuestion`, `PlayerAnswered`, `PointsEarned`, `Ranking`, `ShowResult`, `GameEnded`).
- **8 repositories**, **~15 handlers** CQRS, **1 service** (`IPasswordHasher`).

---

## 2. Équivalences Symfony (sans rien casser)

### 2.1 Mapping des couches

| Couche actuelle | Équivalent Symfony | Remarques |
|-----------------|--------------------|-----------|
| **Domain** | Namespace `App\Domain` (ou module `Domain/`) | Entités PHP (classes), Enums PHP 8.1+. Aucune dépendance vers Symfony ni Doctrine dans le Domain. |
| **Application** | Namespace `App\Application` | Ports (interfaces) + cas d’usage. Handlers = **Services** ou **Message Handlers** (Symfony Messenger pour CQRS). |
| **Infrastructure** | Namespace `App\Infrastructure` | Implémentations des repositories (Doctrine), `PasswordHasher` (Symfony Security ou custom avec `password_hash()`). |
| **Api** | Contrôleurs HTTP + (Mercure ou WebSocket) | Contrôleurs Symfony (ou attributs de route), configuration Security (JWT), CORS. |

Les **dépendances** restent identiques en sens :  
`Domain ← Application ← Api` et `Infrastructure → Domain, Application`.

### 2.2 CQRS en Symfony

- **Symfony Messenger** : bus de messages pour Commandes et Requêtes.
  - Command → `MessageBusInterface::dispatch($command)` → un `MessageHandler` par commande.
  - Query → même principe ou service dédié par query (comme aujourd’hui avec les handlers CQRS).
- Les handlers vivent dans **Application** ; les implémentations concrètes (repositories Doctrine) dans **Infrastructure**.
- Très bonne adéquation avec l’existant : 1 handler C# → 1 handler PHP (ou 1 service invoqué par un controller).

### 2.3 Persistance

- **Doctrine ORM** remplace Entity Framework Core.
- **Entités Domain** : en PHP, sans attributs Doctrine dans le Domain si on veut rester “pur” (approche “Domain sans dépendance”). On peut soit :
  - Garder des entités Doctrine dans Infrastructure et les mapper depuis le Domain (plus de travail, très propre), soit
  - Utiliser des entités Doctrine dans un dossier dédié qui reflètent le Domain (approche pragmatique pour un projet d’étude).
- **MySQL** : support natif ; **SQLite** possible en dev (configuration Doctrine).

### 2.4 Temps réel : Mercure vs WebSocket (équivalent SignalR)

| Besoin actuel (SignalR) | Symfony |
|--------------------------|--------|
| Connexion par “groupe” (session de jeu) | **Mercure** : topics (ex. `session/{id}`). Le client s’abonne à un topic ; le serveur publie sur ce topic. |
| Méthodes “RPC” (JoinAsHost, SubmitAnswer, etc.) | Ces méthodes restent des **actions HTTP** ou un **WebSocket custom** (Ratchet, Symfony WebSocket bundle) qui reçoit des messages et appelle l’Application. |
| Envoi d’événements (GameStarted, ShowQuestion, …) | **Mercure** : `PublisherInterface::publish()` vers un topic. Le front reçoit en SSE (Server-Sent Events). |

- **Option A — Mercure (recommandée pour un projet d’étude)**  
  - Protocole officiel Symfony, simple, basé sur HTTP/SSE.  
  - Les “méthodes” du hub deviennent des **endpoints HTTP** (ex. `POST /api/game/join-as-host`, `POST /api/game/submit-answer`) qui, après traitement par l’Application, publient un événement Mercure sur le topic de la session.  
  - Le front : un client Mercure (JS) qui s’abonne aux topics ; plus de connexion WebSocket “RPC” comme SignalR, mais même résultat fonctionnel (temps réel).

- **Option B — WebSocket (Ratchet / bundle WebSocket)**  
  - Plus proche du modèle SignalR (connexion persistante, méthodes appelables).  
  - Plus de travail (serveur WebSocket séparé ou intégré, auth, gestion des groupes).  
  - Utile si l’objectif pédagogique est “répliquer un hub type SignalR en PHP”.

Pour un **projet d’étude pour apprenants**, Mercure est un bon compromis : moderne, documenté, et aligné avec l’écosystème Symfony.

### 2.5 Authentification

- **JWT** : `lexik/jwt-authentication-bundle` (standard dans l’écosystème Symfony).  
- Claims (userId, rôle admin) : équivalents (custom claims ou UserInterface + roles).  
- Policy “Admin” : rôle Symfony ou voter.

---

## 3. Structure cible proposée (Clean Architecture)

```
backend-symfony/   (ou backend/ après migration)
├── src/
│   ├── Domain/
│   │   ├── Entity/         # User, Quiz, Question, …
│   │   └── Enum/           # GameSessionStatus, QuestionType, QuizStatus
│   ├── Application/
│   │   ├── Command/        # CreateQuiz, JoinGameSession, …
│   │   │   ├── CreateQuiz/
│   │   │   │   ├── CreateQuizCommand.php
│   │   │   │   └── CreateQuizHandler.php
│   │   │   └── …
│   │   ├── Query/          # GetQuizDetail, Login, GetProfile, …
│   │   ├── Port/           # ou Interface/ : IQuizRepository, IPasswordHasher, …
│   │   └── Dto/            # DTOs métier
│   ├── Infrastructure/
│   │   ├── Persistence/    # Doctrine : repositories, Entity (mapping si séparé du Domain)
│   │   └── Service/        # PasswordHasher, MercurePublisher (adaptateur)
│   └── Api/
│       ├── Controller/     # AuthController, QuizController, GameSessionController, …
│       ├── MessageHandler/ # (optionnel si on garde les handlers dans Application)
│       └── Security/       # JWT config, voters
├── config/
├── migrations/
└── …
```

Respect des règles :

- **Domain** : pas de référence à Symfony ni Doctrine.
- **Application** : dépend uniquement de Domain ; définit les ports (interfaces).
- **Infrastructure** : implémente les ports, dépend de Domain + Application.
- **Api** : contrôleurs + config ; appellent Application (handlers / bus), pas le Domain directement.

Un outil comme **Deptrac** peut vérifier que les dépendances entre couches ne sont pas violées (comme en .NET avec des analyseurs).

---

## 4. Faisabilité et effort (ordre de grandeur)

| Lot | Contenu | Difficulté | Commentaire |
|-----|--------|------------|-------------|
| 1 | Projet Symfony, Domain (entités + enums), config Doctrine + MySQL/SQLite | Faible | Entités proches du C# ; enums PHP 8.1. |
| 2 | Ports (interfaces Application) + DTOs | Faible | Transposition directe. |
| 3 | Handlers CQRS (Application) + implémentations repositories (Infrastructure) | Moyenne | Logique métier à recopier ; attention à la gestion des transactions (Doctrine). |
| 4 | API HTTP (contrôleurs, routes, validation) | Moyenne | Beaucoup de routes mais répétitif. |
| 5 | Auth JWT + policy Admin | Faible | Lexik JWT + roles. |
| 6 | Temps réel (Mercure) : endpoints “game” + publication événements | Moyenne | Changement de paradigme (HTTP + Mercure au lieu de hub SignalR) ; front à adapter. |

**Estimation globale** : projet réaliste pour un **travail dirigé** (quelques semaines à quelques mois selon le rythme et le nombre d’apprenants). La Clean Architecture se transpose bien ; le point le plus “décalé” est le remplacement de SignalR par Mercure (ou un WebSocket custom).

---

## 5. Intérêt pour un projet d’étude (apprenants)

### 5.1 Points forts

- **Même architecture** : les apprenants retrouvent Domain / Application / Infrastructure / Api, avec les mêmes règles de dépendances. Idéal pour comparer C# et PHP.
- **CQRS** : Symfony Messenger donne une base standard pour commands/queries ; bonne pratique lisible et testable.
- **Écosystème Symfony** : documentation, bundles (Doctrine, Security, Mercure), communauté. Bon pour la formation PHP.
- **Déploiement** : PHP + MySQL très répandus en hébergement ; Docker possible (image PHP-FPM + Nginx/Apache).

### 5.2 Points de vigilance

- **Double stack** : si le front reste inchangé (appels API + SignalR), il faudra soit garder les mêmes contrats (URLs, JSON, événements), soit adapter le front pour Mercure (topics, format des événements).
- **Temps réel** : expliquer la différence SignalR (WebSocket RPC) vs Mercure (HTTP + SSE + topics) fait partie du projet d’étude.
- **Tests** : conserver des tests d’API (ex. auth, quiz) en PHP (PHPUnit) pour valider la parité fonctionnelle.

---

## 6. Recommandation (sans rien toucher pour l’instant)

- **Migration envisageable** : oui, avec la structure Clean Architecture conservée et une transposition claire Domain → Application → Infrastructure → Api.
- **Choix techniques cohérents** : Symfony 7.x, Doctrine ORM, Symfony Messenger (CQRS), Lexik JWT, Mercure pour le temps réel (ou WebSocket si objectif pédagogique “hub style SignalR”).
- **Prochaine étape (quand vous déciderez d’agir)** :  
  1) Créer un projet Symfony à part (ex. `backend-symfony`) sans modifier l’existant.  
  2) Implémenter Domain puis Application (ports + handlers) en s’appuyant sur `backend/ARCHITECTURE.md` et le code actuel.  
  3) Brancher Infrastructure (Doctrine, repositories, PasswordHasher).  
  4) Exposer l’API (contrôleurs, JWT).  
  5) Ajouter le flux temps réel (Mercure ou WebSocket) et documenter les écarts avec SignalR pour les apprenants.

Ce document reste une **étude** : aucun code n’a été modifié ; il peut servir de base à un cahier des charges ou à un parcours de formation “Migration ASP.NET → Symfony en Clean Architecture”.
