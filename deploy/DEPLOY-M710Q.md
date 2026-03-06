# Déploiement KAPOOT sur m710q (Caddy + Docker déjà en place)

Objectif : ajouter KAPOOT sans modifier les autres apps. Le frontend KAPOOT est sur le réseau Docker **web** (comme Caddy), Caddy fait le reverse proxy et le TLS pour **kapoot.reneleliard.online**.

**Déjà fait sur le m710q** : le bloc Caddy pour `kapoot.reneleliard.online` a été ajouté à `/opt/chaterrie/Caddyfile`.

---

## 1. Sur ta machine (où est le code KAPOOT)

- Vérifier que le DNS pointe bien : **kapoot** en A vers **90.1.1.33** (déjà fait côté Hostinger).
- **`.env` optionnel** : sans fichier `.env`, le `docker-compose` utilise les valeurs par défaut (`MYSQL_ROOT_PASSWORD=kapoot_root_secret`, `MYSQL_PASSWORD=kapoot_secret`). Pour des mots de passe personnalisés, crée un `.env` (voir ci-dessous).

---

## 2. Sur le m710q (ssh m710q)

### 2.1 Récupérer le projet

Si le code est sur Git :

```bash
cd /chemin/où/tu/mets/tes/apps   # ex. /home/user/apps
git clone <url-du-repo-kapoot> kapoot
cd kapoot
```

Ou copier le projet (rsync, scp, etc.) dans un dossier dédié, par ex. `~/apps/kapoot` ou `/opt/kapoot`.

### 2.2 (Optionnel) Fichier `.env` pour les mots de passe MySQL

Si tu veux des mots de passe dédiés au lieu des valeurs par défaut, crée un `.env` dans le dossier du projet sur le serveur :

```bash
cd ~/kapoot   # ou le chemin où est le projet
nano .env
```

Contenu minimal :

```
MYSQL_ROOT_PASSWORD=un_mot_de_passe_root_fort
MYSQL_PASSWORD=un_mot_de_passe_kapoot
```

Enregistre puis relance les conteneurs : `docker compose down && docker compose up -d`.  
**Attention** : si la base MySQL a déjà été créée avec les anciens mots de passe, il faudra supprimer le volume (`docker compose down -v`) pour repartir de zéro (données perdues).

### 2.3 Lancer KAPOOT en Docker

Le `docker-compose.yml` connecte le frontend au réseau **web** (comme Caddy), sans exposer de port sur l’hôte.

```bash
cd /chemin/vers/kapoot
docker compose up -d
```

Vérifier que les conteneurs tournent :

```bash
docker compose ps
```

Le frontend doit être sur le réseau `web` pour que Caddy puisse joindre `kapoot-frontend:80`.

---

## 3. Caddy (déjà configuré)

Le bloc suivant a été **ajouté** à `/opt/chaterrie/Caddyfile` :

```caddy
# KAPOOT
kapoot.reneleliard.online {
    reverse_proxy kapoot-frontend:80
}
```

Pour appliquer la config (sans redémarrer les autres sites) :

```bash
docker exec chaterrie-caddy-1 caddy reload --config /etc/caddy/Caddyfile
```

À faire **après** avoir lancé les conteneurs KAPOOT (`docker compose up -d`), sinon Caddy ne pourra pas joindre `kapoot-frontend`.

---

## 4. Premier administrateur

Pour avoir un compte administrateur (voir tous les utilisateurs/quiz, supprimer, promouvoir en admin) :

- **Option A** : Dans `appsettings.json` (ou variables d’environnement), définir `Admin:InitialAdminEmail` avec l’email du premier admin. Lors de l’**inscription** avec cet email, le compte sera créé avec le rôle admin.
- **Option B** : Pour un compte déjà existant, mettre à jour la base (après déploiement) :
  - **SQLite** : `UPDATE Users SET IsAdmin = 1 WHERE Email = 'ton@email.com';`
  - **MySQL** (Docker) : `docker exec -it kapoot-mysql mysql -ukapoot -p kapoot -e "UPDATE Users SET IsAdmin = 1 WHERE Email = 'ton@email.com';"`

Une fois admin, un lien **Administration** apparaît dans la barre de navigation (dashboard) et permet d’accéder à `/dashboard/admin`.

## 5. Vérifications

- **Depuis l’extérieur** : https://kapoot.reneleliard.online → page d’accueil KAPOOT (Caddy gère le HTTPS).
- **Sur le serveur** : `curl -I http://127.0.0.1:8080` → 200 sans toucher au reste.

---

## 6. Résumé

| Élément | Détail |
|--------|--------|
| Caddy | Bloc ajouté dans `/opt/chaterrie/Caddyfile` ; recharger avec `docker exec chaterrie-caddy-1 caddy reload --config /etc/caddy/Caddyfile` |
| KAPOOT frontend | Réseau Docker **web** (reachable par Caddy en `kapoot-frontend:80`), aucun port exposé sur l’hôte |
| MySQL (KAPOOT) | Interne au stack, pas exposé sur l’hôte |

Aucun port 80/443 ni config des autres sites (cv, chaterrie, reneleliard.online) n’a été modifié.
