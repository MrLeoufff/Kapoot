<template>
  <div class="create-quiz">
    <h1>Créer un quiz</h1>
    <form class="form" @submit.prevent="submit">
      <p v-if="error" class="error">{{ error }}</p>
      <div class="field">
        <label for="title">Titre</label>
        <input id="title" v-model="title" type="text" required />
      </div>
      <div class="field">
        <label for="description">Description</label>
        <textarea id="description" v-model="description" rows="3"></textarea>
      </div>
      <div class="actions">
        <button type="submit" class="btn btn-primary" :disabled="loading">Créer</button>
        <RouterLink to="/dashboard" class="btn btn-ghost">Annuler</RouterLink>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { quizService } from '@/services/quiz.service'

const router = useRouter()
const auth = useAuthStore()
const title = ref('')
const description = ref('')
const loading = ref(false)
const error = ref('')

const ownerId = computed(() => auth.user?.id ?? '')

async function submit() {
  if (!ownerId.value) return
  error.value = ''
  loading.value = true
  try {
    await quizService.create(title.value.trim(), description.value.trim(), ownerId.value)
    await router.push('/dashboard')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Erreur lors de la création.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.create-quiz {
  max-width: 500px;
}

.create-quiz h1 {
  margin-bottom: 1.5rem;
}

.form {
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

.field input,
.field textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
}

.field input:focus,
.field textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}

.error {
  color: var(--color-error);
  margin: 0;
}

.actions {
  display: flex;
  gap: 1rem;
  margin-top: 0.5rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  border: none;
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
}

.btn-ghost:hover {
  color: var(--color-text);
}
</style>
