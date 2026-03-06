<template>
  <div class="auth-page">
    <h1 class="auth-title">Connexion</h1>
    <form class="auth-form" @submit.prevent="submit">
      <p v-if="error" class="error">{{ error }}</p>
      <div class="field">
        <label for="email">Email</label>
        <input id="email" v-model="email" type="email" required autocomplete="email" />
      </div>
      <div class="field">
        <label for="password">Mot de passe</label>
        <input id="password" v-model="password" type="password" required autocomplete="current-password" />
      </div>
      <button type="submit" class="btn btn-primary" :disabled="loading">Se connecter</button>
    </form>
    <p class="footer-link">
      Pas encore de compte ? <RouterLink to="/register">S'inscrire</RouterLink>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

const redirectTo = computed(() => (route.query.redirect as string) || '/dashboard')

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(email.value, password.value)
    await router.push(redirectTo.value)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Connexion impossible.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  max-width: 400px;
  margin: 0 auto;
  animation: fade-in-up 0.4s cubic-bezier(0.22, 1, 0.36, 1) both;
}

.auth-title {
  margin-bottom: 1.5rem;
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  color: var(--color-text-muted);
}

.field input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.field input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(124, 92, 255, 0.2);
}

.error {
  color: var(--color-error);
  margin: 0;
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  border: none;
  margin-top: 0.5rem;
  transition: transform 0.2s ease, box-shadow 0.25s ease, background 0.25s ease;
}

.btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 20px rgba(124, 92, 255, 0.35);
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.footer-link {
  margin-top: 1.5rem;
  color: var(--color-text-muted);
}
</style>
