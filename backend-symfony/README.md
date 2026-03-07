# Kapoot — Backend Symfony

API REST Symfony 7, même contrat que le backend C# (Kapoot). Clean Architecture : **Domain** (entités, enums), **Application** (services, ports), **Infrastructure** (Doctrine), **Api** (contrôleurs fins).

## Prérequis

- PHP 8.2+
- Composer
- MySQL 8 (ou MariaDB) ou SQLite
- Extensions : `ctype`, `iconv`, `json`, `mbstring`, `pdo`, `xml`, `openssl`, `zip`

## Installation

```bash
composer install
cp .env.local.example .env.local
# Éditer .env.local : DATABASE_URL (ex. WAMP : mysql://root:@127.0.0.1:3306/kapoot_symfony?serverVersion=8.0&charset=utf8mb4)
```

## Clés JWT (obligatoire pour le login)

```bash
mkdir -p config/jwt
php bin/console lexik:jwt:generate-keypair
```

(Si la commande demande une passphrase, en mettre une et la recopier dans `.env` : `JWT_PASSPHRASE=...`)

## Base de données

```bash
php bin/console doctrine:database:create --if-not-exists
php bin/console doctrine:schema:create
# ou après modification des entités :
php bin/console doctrine:schema:update --force
```

## Lancer l’API

```bash
php -S localhost:8000 -t public
```

Ou avec Symfony CLI : `symfony server:start`

- API : http://localhost:8000/api  
- Ex. : `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/quizzes`, etc.

## Structure (bonnes pratiques)

- **Contrôleurs** (`Controller/Api/`) : parsing requête → appel **service** → réponse JSON. Pas de logique métier.
- **Services** (`Application/Service/`) : logique métier (validation, cas d’usage).
- **Repositories** : interfaces dans `Application/Repository/`, implémentations Doctrine dans `Infrastructure/Doctrine/Repository/`.
- **Entités** : `Entity/` + enums dans `Enum/`.

## CORS

Configuré pour `/api` (origines autorisées dans `config/packages/nelmio_cors.yaml`). En prod, restreindre `allow_origin`.

## Temps réel (Mercure)

Le jeu en direct (démarrage partie, questions, réponses, classement) utilise **Mercure**. Il faut lancer un hub Mercure en plus de l’API.

1. **Variables** (dans `.env` ou `.env.local`) :
   - `MERCURE_URL` : URL interne du hub (ex. `http://mercure:80/.well-known/mercure` en Docker)
   - `MERCURE_PUBLIC_URL` : URL publique du hub pour le navigateur (ex. `http://localhost:1337/.well-known/mercure`)
   - `MERCURE_JWT_SECRET` : secret partagé avec le hub (même valeur que `MERCURE_PUBLISHER_JWT_KEY` du hub)

2. **Lancer le hub** (ex. avec Docker) :
   ```bash
   docker run -p 1337:80 -e MERCURE_PUBLISHER_JWT_KEY='!ChangeThisMercureHubJWTSecretKey!' -e MERCURE_SUBSCRIBER_JWT_KEY='!ChangeThisMercureHubJWTSecretKey!' dunglas/mercure
   ```
   Puis dans `.env.local` : `MERCURE_PUBLIC_URL=http://localhost:1337/.well-known/mercure` et `MERCURE_URL=http://localhost:1337/.well-known/mercure`.

3. **Front hybride** : le frontend détecte le backend via `GET /api/game/mercure-url`. Si la réponse contient `backend: "symfony"` et une `mercureUrl`, il utilise Mercure ; sinon il utilise SignalR (backend C#).
