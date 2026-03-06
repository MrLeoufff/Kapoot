<template>
  <div class="main-layout">
    <header class="header">
      <RouterLink to="/" class="logo">KAPOOT</RouterLink>
      <nav class="nav">
        <template v-if="auth.isAuthenticated">
          <RouterLink to="/dashboard">Dashboard</RouterLink>
          <RouterLink v-if="auth.user?.isAdmin" to="/dashboard/admin">Administration</RouterLink>
          <span class="user">{{ auth.user?.pseudo }}</span>
          <button type="button" class="btn btn-ghost" @click="auth.logout()">Déconnexion</button>
        </template>
        <template v-else>
          <RouterLink to="/login">Connexion</RouterLink>
          <RouterLink to="/register" class="btn btn-primary">Inscription</RouterLink>
        </template>
      </nav>
    </header>
    <main class="main">
      <Transition name="page" mode="out-in">
        <RouterView v-slot="{ Component }">
          <component :is="Component" />
        </RouterView>
      </Transition>
    </main>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth.store'

const auth = useAuthStore()
</script>

<style scoped>
.main-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
  transition: box-shadow var(--duration-normal, 0.3s) ease;
}

.header:hover {
  box-shadow: 0 2px 20px rgba(0, 0, 0, 0.15);
}

.logo {
  font-weight: 700;
  font-size: 1.25rem;
  letter-spacing: 0.05em;
  color: var(--color-text);
  transition: color 0.25s ease, transform 0.2s ease;
}

.logo:hover {
  color: var(--color-primary);
  text-decoration: none;
  transform: scale(1.02);
}

.nav {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.nav a {
  color: var(--color-text-muted);
  position: relative;
  padding: 0.35rem 0;
  transition: color 0.25s ease;
}

.nav a::after {
  content: '';
  position: absolute;
  left: 0;
  bottom: 0;
  width: 0;
  height: 2px;
  background: var(--color-primary);
  transition: width 0.25s var(--ease-out, cubic-bezier(0.22, 1, 0.36, 1));
}

.nav a:hover,
.nav a.router-link-active {
  color: var(--color-primary);
}

.nav a:hover::after,
.nav a.router-link-active::after {
  width: 100%;
}

.user {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.main {
  flex: 1;
  padding: 2rem 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: var(--radius);
  border: none;
  font-weight: 500;
  transition: transform 0.2s ease, background 0.25s ease, color 0.25s ease, box-shadow 0.25s ease;
}

.btn:hover {
  transform: translateY(-1px);
}

.btn:active {
  transform: translateY(0);
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-primary:hover {
  background: var(--color-primary-hover);
  box-shadow: 0 4px 20px var(--color-primary-glow, rgba(124, 92, 255, 0.35));
}

.btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
}

.btn-ghost:hover {
  color: var(--color-text);
  background: rgba(255, 255, 255, 0.06);
}

/* Transition des pages */
.page-enter-active,
.page-leave-active {
  transition: opacity 0.25s ease, transform 0.25s ease;
}

.page-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.page-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
