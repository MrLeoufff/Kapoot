# Quel fichier .env pour quoi ?

Il y a **deux endroits** où des variables d’environnement sont utilisées. Voici quoi mettre où.

---

## 1. À la racine du projet (pour Docker)

**Fichier :** `.env.dev` (à la **racine** : `c:\Kapoot\.env.dev`)

**À faire :**
1. Copier `.env.dev.example` en `.env.dev`.
2. Renseigner **au minimum** ces deux lignes (MySQL refuse les mots de passe vides) :

```env
MYSQL_ROOT_PASSWORD=ChoisirUnMotDePasseRoot
MYSQL_PASSWORD=ChoisirUnMotDePasseKapoot
```

Remplace `ChoisirUnMotDePasseRoot` et `ChoisirUnMotDePasseKapoot` par vos vrais mots de passe (n’importe quelle chaîne, ex. `MaSuperSecret123`).

**Si vous lancez la stack Symfony** (et pas seulement C#), vous pouvez ajouter (optionnel) :

```env
APP_SECRET=une_cle_secrete_symfony_32_caracteres
MERCURE_JWT_SECRET=!ChangeThisMercureHubJWTSecretKey!
# Premier compte admin : l’inscription avec cet email aura les droits admin
APP_INITIAL_ADMIN_EMAIL=votre@email.com
```

- `APP_SECRET` : une chaîne aléatoire (ex. 32 caractères).
- `MERCURE_JWT_SECRET` : la valeur par défaut du compose suffit en dev ; en prod, mettez une vraie clé secrète.

**Résumé :**  
Pour Docker en dev, il vous faut **un seul fichier à la racine** : `.env.dev`, avec au minimum `MYSQL_ROOT_PASSWORD` et `MYSQL_PASSWORD` remplis.

---

## 2. Dans backend-symfony (pour lancer Symfony sans Docker)

**Fichiers :**  
- `backend-symfony/.env` : déjà présent, valeurs par défaut (en général on ne le modifie pas).  
- `backend-symfony/.env.local` : **c’est ici** que vous mettez vos valeurs locales (base de données, Mercure, etc.).

**Exemple pour travailler en local avec WAMP (MySQL) :**

Dans `backend-symfony/.env.local` :

```env
DATABASE_URL="mysql://root:@127.0.0.1:3306/kapoot_symfony?serverVersion=8.0&charset=utf8mb4"
```

**Exemple si vous lancez aussi Mercure en local** (ex. avec Docker uniquement pour Mercure) :

```env
DATABASE_URL="mysql://root:@127.0.0.1:3306/kapoot_symfony?serverVersion=8.0&charset=utf8mb4"
MERCURE_URL=http://localhost:1337/.well-known/mercure
MERCURE_PUBLIC_URL=http://localhost:1337/.well-known/mercure
MERCURE_JWT_SECRET="!ChangeThisMercureHubJWTSecretKey!"
```

**Résumé :**  
- **Docker (stack complète)** → vous ne touchez qu’au `.env.dev` **à la racine** (point 1).  
- **Symfony seul sur votre machine** (sans Docker pour l’API) → vous éditez `backend-symfony/.env.local` pour la base et éventuellement Mercure.

---

## En bref

| Contexte | Fichier à éditer | Obligatoire |
|----------|------------------|-------------|
| **Docker dev** (C# ou Symfony) | **Racine** : `.env.dev` | `MYSQL_ROOT_PASSWORD`, `MYSQL_PASSWORD` |
| **Symfony en local** (sans Docker API) | `backend-symfony/.env.local` | `DATABASE_URL` (et Mercure si besoin) |

Ne commitez **jamais** `.env.dev` ni `.env.local` (ils sont dans `.gitignore`).
