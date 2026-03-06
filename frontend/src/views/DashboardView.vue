<template>
  <div class="dashboard">
    <h1>Tableau de bord</h1>
    <p class="welcome">Bienvenue, {{ auth.user?.pseudo }}.</p>

    <section class="actions">
      <RouterLink to="/dashboard/quiz/new" class="btn btn-primary">Créer un quiz</RouterLink>
      <button type="button" class="btn btn-outline" @click="showHostModal = true">Présenter un quiz</button>
      <button type="button" class="btn btn-outline" @click="showJoinModal = true">Rejoindre une partie</button>
    </section>

    <section class="my-quizzes">
      <h2>Mes quiz</h2>
      <div v-if="loading" class="loading">Chargement…</div>
      <ul v-else-if="quizzes.length" class="quiz-list">
        <li v-for="q in quizzes" :key="q.id" class="quiz-card">
          <RouterLink :to="`/dashboard/quiz/${q.id}`" class="quiz-link">
            <div class="quiz-info">
              <strong>{{ q.title }}</strong>
              <span class="status" :class="q.status === 1 ? 'published' : 'draft'">
                {{ q.status === 1 ? 'Publié' : 'Brouillon' }}
              </span>
            </div>
            <span class="muted">{{ q.description || 'Sans description' }}</span>
          </RouterLink>
        </li>
      </ul>
      <p v-else class="muted">Vous n'avez pas encore de quiz. Créez-en un !</p>
    </section>

    <div v-if="showHostModal" class="modal-overlay" @click.self="showHostModal = false">
      <div class="modal">
        <h3>Présenter un quiz</h3>
        <p class="muted">Choisissez un quiz publié pour lancer une partie.</p>
        <div v-if="publishedQuizzes.length" class="quiz-select">
          <label for="host-quiz">Quiz</label>
          <select id="host-quiz" v-model="selectedQuizId">
            <option value="">— Sélectionner —</option>
            <option v-for="q in publishedQuizzes" :key="q.id" :value="q.id">{{ q.title }}</option>
          </select>
        </div>
        <p v-else class="muted">Aucun quiz publié. Publiez d’abord un quiz depuis son édition.</p>
        <div class="modal-actions">
          <button type="button" class="btn btn-primary" :disabled="!selectedQuizId || creatingSession" @click="createSession">
            Lancer la partie
          </button>
          <button type="button" class="btn btn-ghost" @click="showHostModal = false">Annuler</button>
        </div>
        <div v-if="sessionCode" class="session-created">
          <p><strong>Partie créée !</strong> Code à donner aux joueurs :</p>
          <p class="code">{{ sessionCode }}</p>
          <RouterLink :to="`/game/host/${sessionCode}`" class="btn btn-primary">Ouvrir l’écran présentateur</RouterLink>
        </div>
      </div>
    </div>

    <div v-if="showJoinModal" class="modal-overlay" @click.self="showJoinModal = false">
      <div class="modal">
        <h3>Rejoindre une partie</h3>
        <p v-if="joinError" class="error">{{ joinError }}</p>
        <div class="field">
          <label for="join-code">Code de la partie</label>
          <input id="join-code" v-model="joinCode" type="text" placeholder="Ex. ABC123" maxlength="10" />
        </div>
        <div class="field">
          <label for="join-pseudo">Votre pseudo</label>
          <input id="join-pseudo" v-model="joinPseudo" type="text" placeholder="Pseudo" />
        </div>
        <div class="modal-actions">
          <button type="button" class="btn btn-primary" :disabled="!joinCode.trim() || !joinPseudo.trim() || joining" @click="joinSession">
            Rejoindre
          </button>
          <button type="button" class="btn btn-ghost" @click="showJoinModal = false">Annuler</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { quizService } from '@/services/quiz.service'
import { gameSessionService } from '@/services/game-session.service'
import type { QuizSummary } from '@/types'

const router = useRouter()
const auth = useAuthStore()
const quizzes = ref<QuizSummary[]>([])
const loading = ref(true)
const ownerId = computed(() => auth.user?.id ?? '')

const showHostModal = ref(false)
const publishedQuizzes = ref<QuizSummary[]>([])
const selectedQuizId = ref('')
const creatingSession = ref(false)
const sessionCode = ref('')

const showJoinModal = ref(false)
const joinCode = ref('')
const joinPseudo = ref(auth.user?.pseudo ?? '')
const joinError = ref('')
const joining = ref(false)

onMounted(async () => {
  if (!ownerId.value) return
  try {
    quizzes.value = await quizService.getMyQuizzes(ownerId.value)
  } catch {
    quizzes.value = []
  } finally {
    loading.value = false
  }
})

watch(showHostModal, async (visible) => {
  if (visible) {
    sessionCode.value = ''
    selectedQuizId.value = ''
    try {
      publishedQuizzes.value = await quizService.getMyQuizzes(ownerId.value)
      publishedQuizzes.value = publishedQuizzes.value.filter((q) => q.status === 1)
    } catch {
      publishedQuizzes.value = []
    }
  }
})

async function createSession() {
  if (!selectedQuizId.value || !auth.user?.id) return
  creatingSession.value = true
  try {
    const res = await gameSessionService.create(selectedQuizId.value, auth.user.id)
    sessionCode.value = res.code
  } catch {
    sessionCode.value = ''
  } finally {
    creatingSession.value = false
  }
}

async function joinSession() {
  const code = joinCode.value.trim().toUpperCase()
  const pseudo = joinPseudo.value.trim()
  if (!code || !pseudo) return
  joinError.value = ''
  joining.value = true
  try {
    const result = await gameSessionService.join({
      code,
      userId: auth.user?.id ?? null,
      pseudo,
    })
    await router.push({
        path: `/game/player/${code}`,
        query: { playerId: result.playerId, pseudo: result.pseudo },
      })
    showJoinModal.value = false
  } catch (e) {
    joinError.value = e instanceof Error ? e.message : 'Impossible de rejoindre la partie.'
  } finally {
    joining.value = false
  }
}
</script>

<style scoped>
.dashboard h1 {
  margin-bottom: 0.5rem;
}

.welcome {
  color: var(--color-text-muted);
  margin-bottom: 2rem;
}

.actions {
  margin-bottom: 2rem;
}

.actions .btn {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  border: none;
  background: var(--color-primary);
  color: white;
  margin-bottom: 0.5rem;
  transition: transform 0.2s ease, box-shadow 0.25s ease, background 0.25s ease;
}

.actions .btn:hover {
  transform: translateY(-1px);
}

.actions .btn-primary:hover {
  box-shadow: 0 4px 20px rgba(124, 92, 255, 0.35);
}

.my-quizzes h2 {
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
  transition: transform 0.25s ease, box-shadow 0.25s ease, border-color 0.25s ease;
}

.quiz-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  border-color: rgba(124, 92, 255, 0.25);
}

.quiz-link {
  display: block;
  color: inherit;
  text-decoration: none;
}

.quiz-link:hover {
  color: inherit;
  text-decoration: none;
}

.quiz-link .muted {
  display: block;
  margin-top: 0.25rem;
}

.actions .btn-outline {
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  border: 1px solid var(--color-primary);
  color: var(--color-primary);
  background: transparent;
  margin-right: 0.5rem;
  margin-bottom: 0.5rem;
}

.actions .btn-outline:hover {
  background: rgba(124, 92, 255, 0.15);
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  animation: fade-in 0.2s ease both;
}

.modal {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.5rem;
  max-width: 420px;
  width: 90%;
  animation: scale-in 0.25s cubic-bezier(0.22, 1, 0.36, 1) both;
}

@keyframes fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes scale-in {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.modal h3 {
  margin: 0 0 0.5rem;
  font-size: 1.25rem;
}

.modal .field {
  margin-bottom: 1rem;
}

.modal label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  color: var(--color-text-muted);
}

.modal input,
.modal select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;
}

.modal .btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
  border: none;
  padding: 0.75rem 1rem;
}

.session-created {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
}

.session-created .code {
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: 0.15em;
  margin: 0.5rem 0 1rem;
}

.quiz-select select {
  margin-top: 0.25rem;
}

.modal .error {
  color: var(--color-error);
  margin-bottom: 0.5rem;
}

.quiz-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}

.status {
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.status.draft {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-text-muted);
}

.status.published {
  background: rgba(34, 197, 94, 0.2);
  color: var(--color-success);
}

.muted {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.loading {
  color: var(--color-text-muted);
}
</style>
