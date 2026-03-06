<template>
  <div class="join-page">
    <section class="join-card">
      <h1>Rejoindre une partie</h1>
      <p class="intro">Entrez le code de la partie communiqué par l’animateur. Aucune connexion requise.</p>
      <form class="join-form" @submit.prevent="submitCode">
        <label for="join-code">Code de la partie</label>
        <input
          id="join-code"
          v-model="gameCode"
          type="text"
          placeholder="Ex. ABC123"
          maxlength="20"
          class="join-input"
          autocomplete="off"
        />
        <p v-if="error" class="error">{{ error }}</p>
        <button type="submit" class="btn btn-primary" :disabled="!gameCode.trim()">
          Continuer
        </button>
      </form>
      <p class="back">
        <RouterLink to="/">← Retour à l’accueil</RouterLink>
      </p>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const gameCode = ref('')
const error = ref('')

function submitCode() {
  const code = gameCode.value.trim().toUpperCase()
  if (!code) return
  error.value = ''
  router.push({ name: 'GamePlayer', params: { code } })
}
</script>

<style scoped>
.join-page {
  min-height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
}

.join-card {
  width: 100%;
  max-width: 400px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 2rem;
  animation: fade-in-up 0.4s ease both;
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

.join-card h1 {
  margin: 0 0 0.5rem;
  font-size: 1.5rem;
}

.intro {
  color: var(--color-text-muted);
  margin: 0 0 1.5rem;
  font-size: 0.95rem;
}

.join-form label {
  display: block;
  margin-bottom: 0.35rem;
  font-weight: 500;
  font-size: 0.9rem;
}

.join-input {
  width: 100%;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  font-size: 1.1rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  background: var(--color-bg);
  color: var(--color-text);
}

.join-input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(124, 92, 255, 0.2);
}

.join-form .error {
  margin-bottom: 0.75rem;
}

.join-form .btn {
  width: 100%;
  padding: 0.75rem 1.25rem;
  font-weight: 600;
}

.back {
  margin: 1.5rem 0 0;
  font-size: 0.9rem;
}

.back a {
  color: var(--color-primary);
  text-decoration: none;
}

.back a:hover {
  text-decoration: underline;
}
</style>
