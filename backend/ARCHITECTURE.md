## Backend KAPOOT – Clean Architecture

### 1. Objectif

Définir une architecture **Clean Architecture** pour le backend KAPOOT, en C# / .NET, qui sépare clairement :

- le **domaine métier** (règles de quiz, parties, scores),
- les **cas d’usage / application** (scripts de création de quiz, lancement de partie, etc.),
- les **détails d’implémentation** (base de données, SignalR, emailing),
- l’**API** exposée au front (HTTP + temps réel).

L’idée clé : les couches **internes** ne dépendent jamais des couches **externes**, uniquement l’inverse.

---

### 2. Couches et dépendances

Arborescence racine backend (déjà créée) :

- `backend/`
  - `src/`
    - `Domain/`
    - `Application/`
    - `Infrastructure/`
    - `Api/`
  - `tests/`

Les dépendances doivent suivre le schéma suivant :

- `Domain` : **aucune dépendance** vers les autres couches.
- `Application` :
  - dépend de `Domain` (connaît les entités métier, les interfaces de repository, etc.).
- `Infrastructure` :
  - dépend de `Domain` (implémente les repositories, accès BDD),
  - dépend de `Application` (implémente les interfaces de services définies dans l’application).
- `Api` :
  - dépend de `Application` (appelle les cas d’usage),
  - peut connaître quelques DTO spécifiques à l’API.

Vue schématique :

```text
Domain       ←  Application       ←        Api
    ↑                 ↑
    └─────────────── Infrastructure
```

Les flèches représentent les dépendances autorisées.

---

### 3. Rôle de chaque couche

#### 3.1. Domain

Contenu typique :

- Entités métier : `User`, `Quiz`, `Question`, `Choice`, `GameSession`, `Player`, `Answer`, `Score`, etc.
- Value Objects éventuels (ex : Email, CodeDePartie).
- Interfaces génériques simples (si besoin, par exemple pour le temps).
- Logique métier **pure** (sans accès direct à la base ou à ASP.NET).

Pas de dépendance vers Entity Framework, ASP.NET Core, ni aucune techno externe.

#### 3.2. Application

Contenu typique :

- **Cas d’usage** (ou “services d’application”, “handlers”) :
  - Créer un quiz,
  - Modifier un quiz,
  - Publier un quiz,
  - Lancer une partie,
  - Rejoindre une partie,
  - Envoyer une réponse à une question,
  - Calculer un classement, etc.
- Interfaces vers l’extérieur (ports) :
  - interfaces de repositories (`IQuizRepository`, `IGameSessionRepository`, etc.),
  - interface pour l’emailing (`IEmailSender`),
  - interface pour la gestion du temps réel (`IGameSessionNotifier`), etc.
- DTO d’entrée/sortie spécifiques à l’application (commandes / résultats).

La couche **Application** orchestre le domaine, mais **ne sait pas** comment la base ou SignalR sont implémentés.

#### 3.3. Infrastructure

Contenu typique :

- Implémentations concrètes de :
  - repositories (avec Entity Framework Core),
  - `IEmailSender` (MailKit / SMTP),
  - `IGameSessionNotifier` (SignalR, ou autres).
- Configuration de la persistance :
  - DbContext EF Core,
  - mapping entités ↔ tables,
  - migrations.

Ces classes **réalisent** les interfaces définies dans `Application` (et éventuellement `Domain`), mais la logique métier reste dans les couches internes.

#### 3.4. Api

Contenu typique :

- Projet ASP.NET Core :
  - `Program.cs` / `Startup` (configuration),
  - contrôleurs ou endpoints minimal API,
  - configuration de SignalR (hubs),
  - configuration d’authentification.
- DTO d’API (requêtes/réponses HTTP), éventuellement différents de ceux de la couche Application.
- Mapping entre DTO API ↔ commandes / DTO Application.

La couche Api ne doit pas contenir de logique métier lourde : elle délègue au maximum à la couche Application.

---

### 4. Projets .NET à créer (quand le SDK sera installé)

Quand le SDK .NET sera installé, l’idée est de créer les projets suivants :

- Solution :
  - `Kapoot.sln`
- Projets :
  - `Kapoot.Domain`        → dossier `backend/src/Domain`
  - `Kapoot.Application`   → dossier `backend/src/Application`
  - `Kapoot.Infrastructure`→ dossier `backend/src/Infrastructure`
  - `Kapoot.Api`           → dossier `backend/src/Api`

Puis configurer les références :

1. `Kapoot.Application` **référence** `Kapoot.Domain`.
2. `Kapoot.Infrastructure` **référence** :
   - `Kapoot.Domain`,
   - `Kapoot.Application`.
3. `Kapoot.Api` **référence** `Kapoot.Application`.

En commandes (à exécuter plus tard, une fois le SDK installé) :

```bash
dotnet new sln -n Kapoot
dotnet new classlib -n Kapoot.Domain -o backend/src/Domain
dotnet new classlib -n Kapoot.Application -o backend/src/Application
dotnet new classlib -n Kapoot.Infrastructure -o backend/src/Infrastructure
dotnet new webapi   -n Kapoot.Api -o backend/src/Api

dotnet sln Kapoot.sln add backend/src/Domain/Kapoot.Domain.csproj
dotnet sln Kapoot.sln add backend/src/Application/Kapoot.Application.csproj
dotnet sln Kapoot.sln add backend/src/Infrastructure/Kapoot.Infrastructure.csproj
dotnet sln Kapoot.sln add backend/src/Api/Kapoot.Api.csproj

dotnet add backend/src/Application/Kapoot.Application.csproj reference backend/src/Domain/Kapoot.Domain.csproj
dotnet add backend/src/Infrastructure/Kapoot.Infrastructure.csproj reference backend/src/Domain/Kapoot.Domain.csproj
dotnet add backend/src/Infrastructure/Kapoot.Infrastructure.csproj reference backend/src/Application/Kapoot.Application.csproj
dotnet add backend/src/Api/Kapoot.Api.csproj reference backend/src/Application/Kapoot.Application.csproj
```

---

### 5. Tests

À terme, on pourra ajouter des projets de tests dans `backend/tests` :

- `Kapoot.Domain.Tests` : tests unitaires sur le domaine pur.
- `Kapoot.Application.Tests` : tests sur les cas d’usage.

Les tests sur l’Infrastructure et l’Api pourront être faits via tests d’intégration.

---

Cette structure te donne une base claire **Clean Architecture**.  
Prochaine étape technique, une fois le SDK .NET installé : créer les projets .NET listés ci-dessus et commencer à remplir `Domain` et `Application` avec les entités et cas d’usage définis dans `DOC.md`.

