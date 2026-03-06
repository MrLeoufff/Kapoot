# Kapoot

Application de quiz interactif en temps réel : création de quiz, lancement de parties (sessions) avec code d’accès, et jeu en direct via SignalR.

## Stack

- **Backend** : ASP.NET Core 8, Clean Architecture, EF Core (MySQL / SQLite), SignalR
- **Frontend** : Vue 3, TypeScript, Vite
- **Base de données** : MySQL 8 (Docker) ou SQLite (dev local sans Docker)

## Démarrer en local (Docker)

1. Cloner le dépôt.
2. Copier `.env.dev.example` en `.env.dev` et renseigner `MYSQL_ROOT_PASSWORD` et `MYSQL_PASSWORD` (valeurs non vides obligatoires).
3. Lancer la stack :

```bash
docker compose --env-file .env.dev -f docker-compose.dev.yml up -d
```

- **App** : http://localhost:8080  
- **API** : http://localhost:5000  
- **MySQL** : `localhost:3306` (user `kapoot`)

Voir **[DOCKER-DEV.md](DOCKER-DEV.md)** pour le détail.

## Documentation

| Fichier | Contenu |
|---------|---------|
| [DOCKER-DEV.md](DOCKER-DEV.md) | Lancer la stack en dev avec Docker |
| [APPRENTISSAGE.md](APPRENTISSAGE.md) | Lecture guidée du projet (architecture, flux, exemples) |
| [backend/ARCHITECTURE.md](backend/ARCHITECTURE.md) | Clean Architecture backend |

## Licence

Projet personnel / éducatif.
