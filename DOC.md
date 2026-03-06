## KAPOOT – Documentation fonctionnelle & technique

### 1. Présentation du projet

**Nom du projet** : KAPOOT  
**Objectif** : Créer une plateforme web de quiz en ligne, inspirée de Kahoot, destinée aux développeurs/apprenants de la communauté « Entraide & Motivation ».  
Le site permet de :
- **Créer** des quiz personnalisés orientés développement web.
- **Présenter** ces quiz en live à un groupe (présentateur + joueurs).
- **Jouer** en temps réel via un lien/code de partie.
- **Suivre** la progression des apprenants (scores, historique simple).

**Public cible** : Apprenants développeurs (débutants à intermédiaires), membres de la communauté.

**Contexte** :  
Permettre aux membres de :
- se situer dans leur progression,
- se tester de façon fun,
- animer des sessions live pédagogiques.

---

### 2. Rôles et parcours utilisateurs

#### 2.1. Créateur

**Rôle** : Conçoit le contenu des quiz.

- Créer un nouveau quiz.
- Ajouter / modifier / supprimer des questions.
- Gérer les réponses possibles et les bonnes réponses.
- Associer une explication à chaque question (affichée après la correction).
- Enregistrer un quiz en **brouillon** puis le **publier**.
- Gérer une bibliothèque personnelle de quiz.

#### 2.2. Présentateur

**Rôle** : Anime une session de quiz en direct.

- Choisir un quiz publié.
- Lancer une **partie** (GameSession) à partir de ce quiz.
- Générer un **code ou lien de partie** à partager aux joueurs.
- Voir la liste des joueurs connectés.
- Contrôler le déroulement :
  - afficher la question courante,
  - terminer la question,
  - afficher les résultats et le classement provisoire,
  - passer à la question suivante.
- Afficher à la fin le **classement final**.

#### 2.3. Joueur

**Rôle** : Participe à une session de quiz.

- Rejoindre une partie via un **lien ou un code**.
- Soit :
  - se connecter avec un compte utilisateur,
  - soit entrer un **pseudo** (joueur anonyme).
- Répondre aux questions en temps limité (optionnel).
- Voir le résultat de chaque question (bonne/mauvaise réponse + explication).
- Voir son score et sa position au classement.

**Contrainte importante** :  
> dès qu'un joueur quitte le lien, il ne doit pas pouvoir re-rejoindre la partie.

#### 2.4. Utilisateur authentifié (profil)

- Peut être créateur, présentateur, joueur.
- Dispose d’un **profil** contenant :
  - pseudo,
  - avatar,
  - nombre de parties jouées,
  - nombre de quiz créés,
  - nombre de quiz présentés,
  - quelques statistiques simples (optionnel).

---

### 3. Fonctionnalités principales (MVP)

Cette section décrit le **MVP** (Minimum Viable Product) à livrer en priorité, en cohérence avec le cahier des charges.

#### 3.1. Gestion des quiz

- Création d’un quiz :
  - titre,
  - description,
  - éventuels tags / catégories (ex : HTML, JS, backend, débutant…),
  - statut : **brouillon** / **publié**.
- Edition / suppression d’un quiz (tant qu’il n’est pas verrouillé par l’historique des parties, à définir).
- Liste des quiz d’un créateur (filtre par statut).
- Liste des quiz publiés (côté présentateur / choix de quiz).

#### 3.2. Types de questions

Pour le MVP :
- **QCM (Choix multiple)**.
- **Vrai/Faux**.

Pour chaque question :
- texte de la question,
- au moins deux propositions,
- marquage des propositions correctes,
- **explication** affichée après la question (afin d’apprendre du quiz).

Optionnel (MVP+) :
- **Glisser-déposer** (drag & drop) pour associer/ordonner des éléments.

#### 3.3. Déroulement d’une partie (live)

1. Le présentateur choisit un quiz publié.
2. Le système crée une **GameSession** et génère un **code de partie** (ou un lien).
3. Les joueurs rejoignent la partie via ce code/lien :
   - en se connectant (compte),
   - ou en entrant un pseudo (joueur anonyme).
4. Le présentateur lance la partie :
   - question 1 s’affiche chez tous les joueurs.
5. Phase réponse :
   - les joueurs répondent,
   - éventuellement un timer (optionnel au début).
6. Fin de la question :
   - le système calcule les résultats,
   - diffuse à tous :
     - la bonne réponse,
     - l’explication,
     - un **classement provisoire**.
7. Fin de la partie (dernière question) :
   - calcul des scores finaux,
   - affichage du **classement final**,
   - possibilité de conserver les résultats pour l’historique.

#### 3.4. Profils & classements

**Profil utilisateur** (authentifié) :
- pseudo, avatar,
- nombre de parties jouées,
- nombre de quiz créés,
- nombre de quiz présentés.

**Classements** :
- par partie : classement live et final,
- top 10 **simple** des quiz (ex : les plus joués).

---

### 4. Fonctionnalités optionnelles (v1+)

Ces fonctionnalités sont à considérer après le MVP, si le temps le permet.

- **Mode entraînement solo**
  - Jouer un quiz seul, sans présentateur.
  - Permet de s’entraîner hors contexte live.

- **Tags / catégories / niveau**
  - Ajouter des tags aux quiz/questions (HTML, JS, backend, etc.).
  - Notion de difficulté : débutant / intermédiaire / avancé.
  - Filtrage des quiz par thème / difficulté.

- **Génération de quiz aléatoire**
  - Générer un quiz en fonction des résultats passés :
    - cibler les faiblesses de l’utilisateur,
    - adapter la difficulté.

- **Stats avancées / analytics**
  - Taux de bonnes réponses par quiz / question.
  - Questions les plus ratées.
  - Courbes de progression d’un utilisateur.

- **Duplication de quiz**
  - Cloner un quiz existant pour le modifier.

- **Mode équipes**
  - Joueurs regroupés en équipes,
  - classement d’équipe en plus du classement individuel.

- **Import / export de quiz**
  - Export des quiz (JSON/CSV),
  - Import permettant de partager des quiz.

- **Anti-triche / robustesse**
  - Expiration du code de partie (validité limitée).
  - Randomisation de l’ordre des questions.
  - Randomisation de l’ordre des réponses.

- **Confort & accessibilité**
  - Thème sombre / clair.
  - Police ajustable.
  - Raccourcis clavier pour le présentateur.

---

### 5. Architecture technique cible

#### 5.1. Backend (C# / .NET)

- **Langage** : C#  
- **Framework** : ASP.NET Core
  - API REST en JSON.
  - Gestion de l’authentification.
  - Gestion de la logique métier (quiz, parties, scoring).
- **Temps réel** : SignalR
  - Gestion des connexions/déconnexions des joueurs.
  - Diffusion des questions.
  - Réception des réponses.
  - Diffusion des scores et des classements.
- **Base de données** : Entity Framework Core
  - Provider possible : SQLite (simple pour démarrer) ou SQL Server.
- **Authentification** :
  - ASP.NET Identity ou système custom,
  - JWT ou cookies.
- **Emails (optionnel)** :
  - MailKit ou SMTP basique (inscription, reset de mot de passe…).

#### 5.2. Frontend (Vue 3 + TypeScript)

- **Framework** : Vue 3
- **Langage** : TypeScript
- **Build / Dev server** : Vite
- **Routing** : Vue Router
- **State management** : Pinia
- **Temps réel** : client SignalR en TypeScript
- **UI** :
  - CSS moderne,
  - éventuellement Tailwind ou une petite librairie de composants.

---

### 6. Modèle de données (premier jet conceptuel)

Ce modèle est une base, ajustable à la mise en œuvre.

- `User`
  - Id
  - Email
  - PasswordHash
  - Pseudo
  - AvatarUrl (optionnel)
  - DateCreated

- `Quiz`
  - Id
  - Title
  - Description
  - Status (Draft / Published)
  - OwnerId (User)
  - CreatedAt
  - UpdatedAt

- `Question`
  - Id
  - QuizId
  - Text
  - Type (MCQ, TrueFalse, DragDrop…)
  - Explanation
  - Order (position dans le quiz)

- `Choice`
  - Id
  - QuestionId
  - Text
  - IsCorrect
  - Order

- `GameSession`
  - Id
  - QuizId
  - HostId (User – présentateur)
  - Code (code/lien de partie)
  - Status (Waiting, Running, Finished)
  - CreatedAt
  - StartedAt
  - FinishedAt

- `Player`
  - Id
  - UserId (optionnel, null si joueur anonyme)
  - GameSessionId
  - Pseudo
  - HasLeft (pour appliquer la contrainte de non-retour)

- `Answer`
  - Id
  - PlayerId
  - QuestionId
  - SelectedChoiceIds (selon le type de question, à modéliser)
  - IsCorrect
  - AnsweredAt

- `Score`
  - Id
  - PlayerId
  - GameSessionId
  - TotalPoints
  - Rank (optionnel, peut être calculé à la volée)

---

### 7. Pages et parcours côté frontend

#### 7.1. Accueil non logué

- Présentation rapide du concept de KAPOOT.
- Mise en avant des bénéfices :
  - quiz personnalisables,
  - côté fun,
  - progression des compétences.
- Boutons :
  - Connexion,
  - Inscription.
- Affichage d’un **top 10** de quiz (simple : les plus joués).

#### 7.2. Authentification

- Page **Inscription** :
  - email,
  - mot de passe,
  - pseudo.
- Page **Connexion** :
  - email,
  - mot de passe.

#### 7.3. Accueil logué / Dashboard

- Vue d’ensemble :
  - liste de mes quiz (créateur),
  - boutons :
    - créer un nouveau quiz,
    - présenter un quiz,
    - rejoindre une partie.
- Bloc recensant :
  - quiz en brouillon,
  - quiz publiés.

#### 7.4. Création / édition de quiz

- Formulaire informations générales :
  - titre,
  - description,
  - tags / catégories (optionnel),
  - difficulté (optionnel).
- Gestion des questions :
  - ajout / modification / suppression,
  - choix du **type de question** (QCM, Vrai/Faux au début),
  - définition des propositions / bonnes réponses,
  - saisie de l’explication.
- Actions :
  - sauvegarde en brouillon,
  - publication.

#### 7.5. Écran présentateur

- Sélection d’un quiz publié.
- Bouton “Lancer une partie” :
  - création d’une GameSession,
  - affichage du code/lien de partie.
- Liste des joueurs connectés.
- Contrôles :
  - démarrer la partie,
  - afficher la question suivante,
  - mettre fin à une question,
  - afficher les résultats et le classement provisoire.
- Fin de partie :
  - classement final,
  - possibilité de relancer ou quitter.

#### 7.6. Écran joueur

- Formulaire pseudo (si anonyme).
- Saisie du code de partie (ou entrée via lien).
- Affichage :
  - question courante,
  - propositions de réponses,
  - état (en attente, question en cours, en correction…).
- Après la question :
  - bonne réponse + explication,
  - feedback sur son score.
- Fin de partie :
  - classement final,
  - score personnel.

#### 7.7. Profil utilisateur

- Affichage :
  - pseudo,
  - avatar,
  - nombre de parties jouées,
  - nombre de quiz créés,
  - nombre de quiz présentés.
- Historique simple (optionnel) :
  - dernières parties jouées,
  - derniers quiz créés.

---

### 8. Todolist structurée (macro)

Cette todolist sert de base pour suivre l’avancement du projet.

#### 8.1. Setup & apprentissage

- [ ] Installer l’environnement C# / .NET (SDK, IDE).
- [ ] Suivre un tutoriel **ASP.NET Core Web API + EF Core**.
- [ ] Installer Node.js + gestionnaire de paquets (npm, pnpm ou yarn).
- [ ] Suivre un tutoriel **Vue 3 + TypeScript + Vite** (avec Vue Router).

#### 8.2. Backend – structure de base

- [ ] Créer le projet **ASP.NET Core Web API**.
- [ ] Ajouter Entity Framework Core au projet.
- [ ] Configurer la base de données (SQLite ou SQL Server local).
- [ ] Implémenter les entités :
  - [ ] `User`
  - [ ] `Quiz`
  - [ ] `Question`
  - [ ] `Choice`
  - [ ] `GameSession`
  - [ ] `Player`
  - [ ] `Answer`
  - [ ] `Score`
- [ ] Créer et appliquer les migrations (création de la base).

#### 8.3. Backend – API REST classique

- [ ] Authentification :
  - [ ] Inscription.
  - [ ] Connexion.
  - [ ] Gestion de l’authentification (JWT ou cookies).
- [ ] API quiz (créateur) :
  - [ ] Créer un quiz (brouillon).
  - [ ] Modifier un quiz.
  - [ ] Supprimer un quiz (selon règles).
  - [ ] Gérer questions et réponses.
  - [ ] Publier un quiz.
  - [ ] Lister les quiz d’un créateur.
- [ ] API lecture de quiz (présentateur / joueur) :
  - [ ] Lister les quiz publiés.
  - [ ] Récupérer le détail d’un quiz.

#### 8.4. Backend – temps réel (SignalR) et parties

- [ ] Ajouter SignalR au projet backend.
- [ ] Créer un `GameHub` (nom à définir).
- [ ] Lancer une partie :
  - [ ] Endpoint ou méthode pour créer une `GameSession` à partir d’un `Quiz`.
  - [ ] Générer et renvoyer le code/lien de partie.
- [ ] Connexion des joueurs :
  - [ ] Permettre la connexion via code.
  - [ ] Associer connexion SignalR ↔ joueur (id utilisateur ou pseudo).
  - [ ] Empêcher le retour d’un joueur ayant quitté (gestion `HasLeft`).
- [ ] Déroulement du quiz :
  - [ ] Diffuser la question courante.
  - [ ] Recevoir les réponses des joueurs.
  - [ ] Calculer les résultats (bonne réponse, points).
  - [ ] Diffuser le classement provisoire.
  - [ ] Clôturer la partie et diffuser le classement final.

#### 8.5. Backend – profils & stats simples

- [ ] Endpoint profil utilisateur :
  - [ ] Récupération pseudo, avatar, stats basiques.
- [ ] Endpoint top 10 quiz :
  - [ ] Critère simple (ex : nombre de parties jouées).

#### 8.6. Frontend – setup

- [ ] Créer projet **Vue 3 + TypeScript + Vite**.
- [ ] Installer et configurer **Vue Router**.
- [ ] Installer et configurer **Pinia**.
- [ ] Créer un module d’appels API (Axios ou `fetch`).
- [ ] Installer et configurer le client **SignalR**.

#### 8.7. Frontend – pages & parcours

- [ ] Accueil non logué :
  - [ ] Présentation du concept.
  - [ ] Boutons Connexion / Inscription.
  - [ ] Top 10 quiz.
- [ ] Authentification :
  - [ ] Page inscription.
  - [ ] Page connexion.
- [ ] Dashboard (accueil logué) :
  - [ ] Liste des quiz créés.
  - [ ] Bouton “Créer un quiz”.
  - [ ] Bouton “Présenter un quiz”.
  - [ ] Interface “Rejoindre une partie”.
- [ ] Création / édition de quiz :
  - [ ] Formulaire infos générales.
  - [ ] Gestion des questions (CRUD).
  - [ ] Sélection du type de question (QCM, Vrai/Faux).
  - [ ] Enregistrement en brouillon.
  - [ ] Publication.
- [ ] Écran présentateur :
  - [ ] Sélection d’un quiz.
  - [ ] Lancement d’une partie (affichage du code).
  - [ ] Affichage des joueurs connectés.
  - [ ] Contrôles du déroulement (questions, corrections).
  - [ ] Affichage classement provisoire et final.
- [ ] Écran joueur :
  - [ ] Saisie pseudo (si anonyme).
  - [ ] Entrée code de partie / redirection via lien.
  - [ ] Affichage des questions et réponses.
  - [ ] Affichage des résultats et du score.
- [ ] Profil :
  - [ ] Affichage pseudo, avatar.
  - [ ] Affichage des stats basiques.

#### 8.8. Frontend – UX, design & responsive

- [ ] Définir une palette de couleurs et une typographie cohérentes.
- [ ] Mise en page responsive (mobile, tablette, desktop).
- [ ] Ajout d’animations légères (ex : transitions sur les scores).
- [ ] Gestion des cas d’erreur (partie inexistante, partie terminée, etc.).

#### 8.9. Tests, validation et déploiement

- [ ] Définir des scénarios de test (créateur, présentateur, joueur).
- [ ] Tester le temps réel avec plusieurs utilisateurs en même temps.
- [ ] Corriger les bugs bloquants.
- [ ] Préparer un déploiement minimal :
  - [ ] Déploiement backend .NET.
  - [ ] Build frontend (`npm run build`) et hébergement.

---

Ce document sert de base de travail pour :
- cadrer le périmètre fonctionnel,
- organiser les développements,
- prioriser les features (MVP vs optionnel).

Il pourra être affiné au fur et à mesure de l’avancement du projet.

