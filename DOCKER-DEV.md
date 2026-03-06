# Mode développement avec Docker (à partager avec l’équipe)

Pour faire tourner Kapoot en dev avec la même stack que la prod (MySQL + API + frontend), sans réseau externe ni sous-domaine. Voir **DOCKER.md** pour la mise en production.

## Prérequis

- Docker et Docker Compose installés

## Variables d’environnement (obligatoire)

Aucune donnée sensible n’est dans les fichiers compose. Il faut un fichier `.env.dev` à la racine :

1. Copier le template : `cp .env.dev.example .env.dev` (ou sous Windows : copier `.env.dev.example` en `.env.dev`)
2. Renseigner `MYSQL_ROOT_PASSWORD` et `MYSQL_PASSWORD` dans `.env.dev` (valeurs non vides obligatoires pour MySQL). Ne jamais committer `.env.dev`.

## Lancer la stack dev

À la racine du projet (`C:\Kapoot`) :

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

## Accès

| Cible                           | URL                                                                               |
| ------------------------------- | --------------------------------------------------------------------------------- |
| **Application**                 | http://localhost:8080                                                             |
| **API directe** (Postman, etc.) | http://localhost:5000                                                             |
| **MySQL**                       | `localhost:3306` (user `kapoot`, mot de passe = `MYSQL_PASSWORD` dans `.env.dev`) |

Les conteneurs dev ont des noms et un volume dédiés (`mysql-data-dev`) pour ne pas mélanger avec la prod. L’API tourne en `ASPNETCORE_ENVIRONMENT=Development`.

## Commandes utiles

| Commande                                                             | Description                            |
| -------------------------------------------------------------------- | -------------------------------------- |
| `docker compose --env-file .env.dev -f docker-compose.dev.yml up -d` | Démarre la stack dev                   |
| `docker compose -f docker-compose.dev.yml down`                      | Arrête les conteneurs dev              |
| `docker compose -f docker-compose.dev.yml down -v`                   | Arrête et supprime le volume MySQL dev |
| `docker compose -f docker-compose.dev.yml logs -f api`               | Logs de l’API                          |
| `docker compose -f docker-compose.dev.yml build --no-cache`          | Reconstruire les images sans cache     |

Le schéma (tables) est créé automatiquement au premier démarrage de l’API (`EnsureCreated()`).
