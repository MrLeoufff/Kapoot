# Architecture frontend – KAPOOT

Vue 3 + TypeScript + Vite, organisé en couches claires (bonnes pratiques / clean architecture).

## Structure des dossiers

```
src/
├── main.ts              # Point d'entrée, Pinia + Router
├── App.vue              # Racine, RouterView
├── env.d.ts             # Types Vue + env Vite
├── types/               # Types partagés (alignés API)
│   └── index.ts
├── services/            # Couche "infrastructure" – appels API
│   ├── api/
│   │   └── http.ts      # Client HTTP (base URL, token, erreurs)
│   ├── auth.service.ts
│   ├── quiz.service.ts
│   ├── game-session.service.ts
│   └── profile.service.ts
├── stores/              # État global (Pinia)
│   └── auth.store.ts
├── router/
│   └── index.ts        # Routes + garde auth/guest
├── layouts/
│   └── MainLayout.vue  # En-tête + RouterView
├── views/              # Pages (une par route principale)
│   ├── HomeView.vue
│   ├── LoginView.vue
│   ├── RegisterView.vue
│   ├── DashboardView.vue
│   ├── CreateQuizView.vue
│   └── NotFoundView.vue
└── styles/
    └── main.css        # Variables CSS, reset, base
```

## Principes

- **Views** : affichage et interactions utilisateur ; elles utilisent les **stores** et les **services**, pas de logique métier lourde.
- **Services** : seul endroit où l’on appelle l’API (via `http`). Un service par domaine (auth, quiz, game-session, profile).
- **Stores (Pinia)** : état global (ex. utilisateur connecté, token). Persistance optionnelle (localStorage pour auth).
- **Router** : routes + `meta.requiresAuth` / `guestOnly` pour protéger les pages.
- **Types** : interfaces partagées, alignées avec les DTOs du backend.

## Développement

- Backend à lancer sur le port **5000** (ou adapter le proxy dans `vite.config.ts`).
- En dev, le proxy Vite redirige `/api` et `/hubs` vers `http://localhost:5000`.
- Lancer le front : `npm run dev` (port 5173).

## Variables d'environnement

- `VITE_API_BASE_URL` : en dev avec proxy, laisser vide. En production, indiquer l’URL complète de l’API si différente de l’origine.
