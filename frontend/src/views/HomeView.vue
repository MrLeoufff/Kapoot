<template>
  <div class="home">
    <section class="hero">
      <h1 class="hero-title">KAPOOT</h1>
      <p class="tagline">Quiz fun, personnalisables et progressifs.</p>
      <p class="sub">Créez vos quiz, lancez des parties en direct, suivez les scores.</p>
      <div class="cta" v-if="!auth.isAuthenticated">
        <RouterLink to="/join" class="btn btn-primary">Rejoindre une partie</RouterLink>
        <RouterLink to="/login" class="btn btn-outline">Connexion</RouterLink>
        <RouterLink to="/register" class="btn btn-outline">Inscription</RouterLink>
      </div>
      <div class="cta" v-else>
        <RouterLink to="/dashboard" class="btn btn-primary">Aller au tableau de bord</RouterLink>
        <RouterLink to="/join" class="btn btn-outline">Rejoindre une partie</RouterLink>
      </div>
    </section>
    <section class="top-quizzes">
      <h2>Top 10 des quiz les plus joués</h2>
      <div v-if="loading" class="loading">Chargement…</div>
      <ul v-else-if="topQuizzes.length" class="quiz-list">
        <li v-for="(q, i) in topQuizzes" :key="q.id" class="quiz-card" :style="{ animationDelay: `${i * 0.06}s` }">
          <RouterLink :to="`/dashboard`">{{ q.title }}</RouterLink>
          <span class="muted">{{ q.description || 'Sans description' }}</span>
        </li>
      </ul>
      <p v-else class="muted">Aucun quiz pour le moment.</p>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { quizService } from '@/services/quiz.service'
import type { QuizSummary } from '@/types'

const auth = useAuthStore()
const topQuizzes = ref<QuizSummary[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    topQuizzes.value = await quizService.getTop10()
  } catch {
    topQuizzes.value = []
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.hero {
  text-align: center;
  padding: 3rem 1rem;
}

.hero-title {
  font-size: 2.5rem;
  margin: 0 0 0.5rem;
  letter-spacing: 0.05em;
  animation: fade-in-up 0.6s var(--ease-out, ease-out) both;
}

.tagline {
  font-size: 1.25rem;
  color: var(--color-primary);
  margin: 0 0 0.5rem;
  animation: fade-in-up 0.5s var(--ease-out, ease-out) 0.1s both;
}

.sub {
  color: var(--color-text-muted);
  margin: 0 0 2rem;
  animation: fade-in-up 0.5s var(--ease-out, ease-out) 0.2s both;
}

.cta {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
  animation: fade-in-up 0.5s var(--ease-out, ease-out) 0.3s both;
}

.cta .btn {
  transition: transform 0.2s ease, box-shadow 0.25s ease, background 0.25s ease, color 0.25s ease;
}

.cta .btn:hover {
  transform: translateY(-2px);
}

.cta .btn-primary:hover {
  box-shadow: 0 6px 24px rgba(124, 92, 255, 0.4);
}

.cta .btn-outline:hover {
  box-shadow: 0 4px 16px rgba(124, 92, 255, 0.2);
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

.btn-primary:hover {
  background: var(--color-primary-hover);
}

.btn-outline {
  background: transparent;
  color: var(--color-primary);
  border: 2px solid var(--color-primary);
}

.btn-outline:hover {
  background: rgba(124, 92, 255, 0.15);
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.top-quizzes {
  margin-top: 3rem;
}

.top-quizzes h2 {
  font-size: 1.25rem;
  margin-bottom: 1rem;
}

.quiz-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 1rem;
}

.quiz-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1rem 1.25rem;
  animation: fade-in-up 0.4s var(--ease-out, ease-out) both;
  transition: transform 0.25s ease, box-shadow 0.25s ease, border-color 0.25s ease;
}

.quiz-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 28px rgba(0, 0, 0, 0.25);
  border-color: rgba(124, 92, 255, 0.3);
}

.quiz-card a {
  font-weight: 600;
  display: block;
  margin-bottom: 0.25rem;
  transition: color 0.2s ease;
}

.quiz-card:hover a {
  color: var(--color-primary);
}

.muted {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.loading {
  color: var(--color-text-muted);
}
</style>
