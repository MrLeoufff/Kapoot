# Mode développement avec Docker

Pour faire tourner Kapoot en dev avec la même stack que la prod (MySQL + API + frontend). Deux backends possibles : **C# (SignalR)** ou **Symfony (Mercure)**. Voir **DOCKER.md** pour la mise en production.

## Prérequis

- Docker et Docker Compose installés

## Variables d’environnement (obligatoire)

Aucune donnée sensible n’est dans les fichiers compose. Il faut un fichier **`.env.dev`** à la racine du projet :

1. Copier le template : `cp .env.dev.example .env.dev` (sous Windows : copier `.env.dev.example` en `.env.dev`)
2. Renseigner dans `.env.dev` :
   - `MYSQL_ROOT_PASSWORD` et `MYSQL_PASSWORD` (valeurs non vides pour MySQL)
   - Pour la stack **Symfony** : `MERCURE_JWT_SECRET` (optionnel), `APP_SECRET`, et **`APP_INITIAL_ADMIN_EMAIL`** (l’email du premier compte à inscrire qui aura les droits admin, comme le backend C#)

Ne jamais committer `.env.dev`.

## Choix du backend

| Stack | Fichier Compose | Backend | Temps réel |
|-------|-----------------|---------|------------|
| **C#** | `docker-compose.dev.yml` | API .NET 8 + SignalR | SignalR (`/hubs/game`) |
| **Symfony** | `docker-compose.dev-symfony.yml` | API Symfony 7 + Mercure | Mercure (SSE via frontend proxy) |

Une seule stack à la fois (même MySQL dev, même ports). Arrêter l’une avant de lancer l’autre si vous basculez.

## Lancer la stack dev (backend C#)

À la racine du projet :

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

## Lancer la stack dev (backend Symfony)

À la racine du projet :

```bash
docker compose --env-file .env.dev -f docker-compose.dev-symfony.yml up -d
```

Au premier démarrage, les clés JWT (Lexik) sont générées automatiquement dans le conteneur.

**Important :** utilisez bien le fichier **docker-compose.dev-symfony.yml** pour lancer la stack Symfony. Si vous aviez auparavant lancé la stack C# (docker-compose.dev.yml), arrêtez-la puis reconstruisez et relancez la stack Symfony :

```bash
docker compose -f docker-compose.dev.yml down
docker compose --env-file .env.dev -f docker-compose.dev-symfony.yml build --no-cache api
docker compose --env-file .env.dev -f docker-compose.dev-symfony.yml up -d
```

Ensuite **créez le schéma Doctrine une fois** (tables `users`, `quizzes`, etc.) :

```bash
docker compose --env-file .env.dev -f docker-compose.dev-symfony.yml exec api sh -c "cd /var/www/html && php bin/console doctrine:schema:create"
```

Sans cela, la table `users` n’existe pas et l’appli Symfony ne peut pas fonctionner. (Vous pouvez aussi utiliser `doctrine:migrations:migrate` si vous utilisez les migrations.)

## Accès (commun aux deux stacks)

| Cible | URL |
|-------|-----|
| **Application** | http://localhost:8080 |
| **API directe** (Postman, etc.) | http://localhost:5000 |
| **MySQL** | `localhost:3306` (user `kapoot`, mot de passe = `MYSQL_PASSWORD` dans `.env.dev`) |

Avec la stack Symfony, le frontend détecte automatiquement le backend (GET `/api/game/mercure-url`) et utilise Mercure ; avec la stack C#, il utilise SignalR.

## Commandes utiles

| Commande | Description |
|----------|-------------|
| `docker compose --env-file .env.dev -f docker-compose.dev.yml up -d` | Démarre la stack dev **C#** |
| `docker compose --env-file .env.dev -f docker-compose.dev-symfony.yml up -d` | Démarre la stack dev **Symfony** |
| `docker compose -f docker-compose.dev.yml down` | Arrête les conteneurs dev C# |
| `docker compose -f docker-compose.dev-symfony.yml down` | Arrête les conteneurs dev Symfony |
| `docker compose -f docker-compose.dev.yml down -v` | Arrête et supprime le volume MySQL dev |
| `docker compose -f docker-compose.dev.yml logs -f api` | Logs de l’API C# |
| `docker compose -f docker-compose.dev-symfony.yml logs -f api` | Logs de l’API Symfony |
| `docker compose -f docker-compose.dev.yml build --no-cache` | Reconstruire les images (C#) sans cache |
| `docker compose -f docker-compose.dev-symfony.yml build --no-cache` | Reconstruire les images (Symfony) sans cache |

Avec la stack C#, le schéma MySQL est créé automatiquement au premier démarrage de l’API (`EnsureCreated()`). Avec Symfony, exécuter une fois `doctrine:schema:create` (ou les migrations) comme indiqué ci-dessus.
