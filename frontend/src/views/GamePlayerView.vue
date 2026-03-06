<template>
  <div class="game-player">
    <div class="player-header">
      <h1>Partie : {{ code }}</h1>
      <p v-if="pseudo" class="pseudo">Connecté en tant que {{ pseudo }}</p>
    </div>

    <!-- Rappel pour mobile : ne pas quitter la page -->
    <p v-if="connected && !gameEnded" class="mobile-warning">
      📱 Sur téléphone ou tablette : gardez cette page ouverte (ne la quittez pas et ne changez pas d’onglet), sinon vous serez déconnecté de la partie.
    </p>

    <!-- Pas encore rejoint : formulaire pseudo (sans compte requis) -->
    <section v-if="!playerId && code" class="join-section">
      <p class="join-intro">Entrez votre pseudo pour rejoindre la partie. Aucune connexion requise.</p>
      <p class="join-mobile-hint">📱 Si vous jouez sur portable, gardez cette page ouverte pendant la partie, sinon vous serez déconnecté.</p>
      <form class="join-form" @submit.prevent="submitJoin">
        <label for="join-pseudo">Votre pseudo</label>
        <input
          id="join-pseudo"
          v-model="joinPseudo"
          type="text"
          placeholder="Ex. Jean"
          maxlength="50"
          required
          class="join-input"
        />
        <p v-if="joinError" class="error">{{ joinError }}</p>
        <button type="submit" class="btn btn-primary" :disabled="joining || !joinPseudo.trim()">
          {{ joining ? 'En cours…' : 'Rejoindre la partie' }}
        </button>
      </form>
    </section>

    <!-- Code manquant : rediriger vers la page "rejoindre" -->
    <section v-else-if="!code" class="join-section">
      <p class="muted">Code de la partie manquant.</p>
      <RouterLink to="/join" class="btn btn-outline">Rejoindre une partie</RouterLink>
    </section>

    <p v-else-if="!connected && !connectionError" class="muted">Connexion en cours…</p>
    <p v-else-if="connectionError" class="error">{{ connectionError }}</p>
    <template v-else>
      <section v-if="currentQuestion" class="question-section">
        <p class="question-text">{{ currentQuestion.text }}</p>
        <p v-if="currentQuestion.allowMultiple" class="multi-hint">Plusieurs réponses possibles — sélectionnez toutes les bonnes réponses puis validez.</p>
        <ul v-if="currentQuestion.allowMultiple" class="choices choices-multi">
          <li v-for="c in currentQuestion.choices" :key="c.id" class="choice-item">
            <label class="choice-label">
              <input
                v-model="selectedChoiceIds"
                type="checkbox"
                :value="c.id"
                :disabled="answerSent"
                class="choice-checkbox"
              />
              <span class="choice-text">{{ c.text }}</span>
            </label>
          </li>
        </ul>
        <ul v-else class="choices">
          <li v-for="c in currentQuestion.choices" :key="c.id">
            <button
              type="button"
              class="choice-btn"
              :disabled="answerSent"
              @click="submitChoice(c.id)"
            >
              {{ c.text }}
            </button>
          </li>
        </ul>
        <div v-if="currentQuestion.allowMultiple && !answerSent" class="validate-row">
          <button
            type="button"
            class="btn btn-primary"
            :disabled="selectedChoiceIds.length === 0"
            @click="submitMultipleChoices"
          >
            Valider
          </button>
        </div>
      </section>
      <section v-else-if="!gameEnded" class="waiting-section">
        <p class="muted">{{ waitingMessage }}</p>
      </section>

      <!-- Écran de fin de partie -->
      <Transition name="game-ended" mode="out-in">
        <section v-if="gameEnded" class="game-ended-section">
          <div class="game-ended-header">
            <h2 class="game-ended-title">Partie terminée !</h2>
            <p class="game-ended-sub">Merci d’avoir joué.</p>
          </div>
          <div v-if="myResult" class="my-result-card" :class="'rank-' + myResult.rank">
            <span class="my-result-label">Ton score</span>
            <span class="my-result-rank">{{ myResult.rank }}{{ myResult.rank === 1 ? 'er' : 'e' }}</span>
            <span class="my-result-points">{{ myResult.totalPoints }} pts</span>
          </div>
          <div v-if="ranking.length" class="final-ranking">
            <h3>Classement final</h3>
            <ul class="final-ranking-list">
              <li
                v-for="(r, i) in ranking"
                :key="r.playerId"
                class="final-ranking-item"
                :class="{ 'is-me': r.playerId === playerId, 'podium': i < 3 }"
              >
                <span class="final-rank">{{ i + 1 }}</span>
                <span class="final-pseudo">{{ r.pseudo }}</span>
                <span class="final-points">{{ r.totalPoints }} pts</span>
              </li>
            </ul>
          </div>
          <div class="game-ended-actions">
            <RouterLink to="/" class="btn btn-primary">Retour à l’accueil</RouterLink>
            <RouterLink to="/join" class="btn btn-outline">Rejoindre une autre partie</RouterLink>
          </div>
        </section>
      </Transition>

      <Transition name="points-toast">
        <div v-if="pointsEarnedDisplay" class="points-toast" :class="{ correct: pointsEarnedDisplay.points > 0 }">
          <span class="points-value">+{{ pointsEarnedDisplay.points }} pts</span>
          <span v-if="pointsEarnedDisplay.rank === 1" class="rank-badge">1ère !</span>
          <span v-else-if="pointsEarnedDisplay.rank === 2" class="rank-badge">2e</span>
          <span v-else-if="pointsEarnedDisplay.rank === 3" class="rank-badge">3e</span>
        </div>
      </Transition>
      <section v-if="ranking.length && !gameEnded" class="ranking">
        <h2>Classement</h2>
        <ol>
          <li v-for="(r, i) in ranking" :key="r.playerId">
            {{ i + 1 }}. {{ r.pseudo }} — {{ r.totalPoints }} pts
          </li>
        </ol>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import * as signalR from '@microsoft/signalr'
import { createGameHubConnection } from '@/services/game-hub.service'
import { gameSessionService } from '@/services/game-session.service'
import { useAuthStore } from '@/stores/auth.store'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const code = computed(() => String(route.params.code ?? '').toUpperCase())
const playerId = computed(() => (route.query.playerId as string) ?? '')

const pseudo = ref('')
const connection = ref<signalR.HubConnection | null>(null)
const connected = ref(false)
const connectionError = ref('')

const joinPseudo = ref('')
const joinError = ref('')
const joining = ref(false)

interface QuestionChoice {
  id: string
  text: string
  order: number
}

interface ShowQuestionPayload {
  questionId: string
  text: string
  type: number
  order: number
  choices: QuestionChoice[]
  allowMultiple?: boolean
}

const currentQuestion = ref<ShowQuestionPayload | null>(null)
const answerSent = ref(false)
const gameEnded = ref(false)
const selectedChoiceIds = ref<string[]>([])
const pointsEarnedDisplay = ref<{ points: number; rank: number } | null>(null)
let pointsEarnedTimeout: ReturnType<typeof setTimeout> | null = null

interface RankingEntry {
  playerId: string
  totalPoints: number
  rank?: number
  pseudo: string
}

const ranking = ref<RankingEntry[]>([])

function normalizeRanking(payload: unknown): RankingEntry[] {
  if (!Array.isArray(payload)) return []
  return payload.map((item: unknown) => {
    const o = item as Record<string, unknown>
    return {
      playerId: String(o.playerId ?? o.PlayerId ?? ''),
      totalPoints: Number(o.totalPoints ?? o.TotalPoints ?? 0),
      rank: o.rank !== undefined || o.Rank !== undefined ? Number(o.rank ?? o.Rank) : undefined,
      pseudo: String(o.pseudo ?? o.Pseudo ?? ''),
    }
  })
}

const waitingMessage = computed(() => {
  if (gameEnded.value) return 'Partie terminée.'
  if (currentQuestion.value) return ''
  return 'En attente de la prochaine question…'
})

const myResult = computed(() => {
  const pid = playerId.value
  if (!pid || !ranking.value.length) return null
  const idx = ranking.value.findIndex((r) => String(r.playerId) === String(pid))
  if (idx < 0) return null
  const r = ranking.value[idx]
  return { rank: idx + 1, totalPoints: r.totalPoints }
})

async function startConnection() {
  const pid = playerId.value
  if (!pid) return
  const conn = createGameHubConnection(pid)
  connection.value = conn

  conn.on('ShowQuestion', (payload: ShowQuestionPayload) => {
    currentQuestion.value = payload
    answerSent.value = false
    selectedChoiceIds.value = []
    pointsEarnedDisplay.value = null
  })

  conn.on('PointsEarned', (payload: { playerId?: string; PlayerId?: string; pointsEarned?: number; PointsEarned?: number; rank?: number; Rank?: number }) => {
    const id = payload.playerId ?? payload.PlayerId ?? ''
    if (id !== playerId.value) return
    const points = payload.pointsEarned ?? payload.PointsEarned ?? 0
    const rank = payload.rank ?? payload.Rank ?? 0
    if (pointsEarnedTimeout) clearTimeout(pointsEarnedTimeout)
    if (points > 0) {
      pointsEarnedDisplay.value = { points, rank }
      pointsEarnedTimeout = setTimeout(() => {
        pointsEarnedDisplay.value = null
        pointsEarnedTimeout = null
      }, 3000)
    } else {
      pointsEarnedDisplay.value = null
    }
  })

  conn.on('ShowResult', () => {
    currentQuestion.value = null
  })

  conn.on('Ranking', (payload: unknown) => {
    ranking.value = normalizeRanking(payload)
  })

  conn.on('GameEnded', (payload: unknown) => {
    ranking.value = normalizeRanking(payload)
    gameEnded.value = true
    currentQuestion.value = null
  })

  try {
    await conn.start()
    await conn.invoke('JoinAsPlayer', code.value, pid)
    connected.value = true
    connectionError.value = ''
    pseudo.value = (route.query.pseudo as string) || 'Joueur'
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Connexion impossible.'
    connected.value = false
  }
}

async function submitJoin() {
  const trimPseudo = joinPseudo.value.trim()
  if (!trimPseudo || !code.value) return
  joinError.value = ''
  joining.value = true
  try {
    const result = await gameSessionService.join({
      code: code.value,
      pseudo: trimPseudo,
      userId: auth.user?.id ?? undefined,
    })
    await router.replace({
      path: route.path,
      query: { ...route.query, playerId: result.playerId, pseudo: result.pseudo },
    })
    joinError.value = ''
    joinPseudo.value = ''
    await startConnection()
  } catch (e) {
    joinError.value = e instanceof Error ? e.message : 'Impossible de rejoindre la partie.'
  } finally {
    joining.value = false
  }
}

onMounted(async () => {
  if (playerId.value) {
    await startConnection()
  }
})

onUnmounted(async () => {
  if (pointsEarnedTimeout) {
    clearTimeout(pointsEarnedTimeout)
    pointsEarnedTimeout = null
  }
  const conn = connection.value
  if (conn) {
    try {
      await conn.stop()
    } catch {
      // ignore
    }
    connection.value = null
  }
  connected.value = false
})

async function submitChoice(choiceId: string) {
  if (!currentQuestion.value || !connection.value || answerSent.value) return
  answerSent.value = true
  try {
    await connection.value.invoke('SubmitAnswer', currentQuestion.value.questionId, [choiceId])
  } catch {
    answerSent.value = false
  }
}

async function submitMultipleChoices() {
  if (!currentQuestion.value || !connection.value || answerSent.value || selectedChoiceIds.value.length === 0) return
  answerSent.value = true
  try {
    await connection.value.invoke('SubmitAnswer', currentQuestion.value.questionId, selectedChoiceIds.value)
  } catch {
    answerSent.value = false
  }
}
</script>

<style scoped>
.game-player {
  max-width: 560px;
}

.player-header {
  margin-bottom: 1.5rem;
}

.player-header h1 {
  margin: 0;
  font-size: 1.25rem;
}

.pseudo {
  margin: 0.25rem 0 0;
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.join-section {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  animation: fade-in-up 0.35s ease both;
}

.join-intro {
  margin: 0 0 1rem;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.join-mobile-hint {
  margin: 0 0 1rem;
  padding: 0.5rem 0.75rem;
  background: rgba(255, 193, 7, 0.15);
  border-radius: var(--radius);
  color: var(--color-text);
  font-size: 0.9rem;
}

.mobile-warning {
  margin: 0 0 1rem;
  padding: 0.5rem 0.75rem;
  background: rgba(255, 193, 7, 0.15);
  border-radius: var(--radius);
  color: var(--color-text);
  font-size: 0.9rem;
}

.join-form label {
  display: block;
  margin-bottom: 0.35rem;
  font-weight: 500;
  font-size: 0.9rem;
}

.join-form .join-input {
  width: 100%;
  padding: 0.6rem 0.75rem;
  margin-bottom: 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  font-size: 1rem;
  background: var(--color-bg);
  color: var(--color-text);
}

.join-form .join-input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(124, 92, 255, 0.2);
}

.join-form .btn {
  padding: 0.6rem 1.25rem;
  font-weight: 600;
}

.join-form .error {
  margin-bottom: 0.75rem;
}

.question-section {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  animation: fade-in-up 0.35s cubic-bezier(0.22, 1, 0.36, 1) both;
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.question-text {
  font-size: 1.1rem;
  margin: 0 0 1rem;
}

.multi-hint {
  font-size: 0.9rem;
  color: var(--color-text-muted);
  margin: 0 0 0.75rem;
}

.choices {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.choices-multi {
  gap: 0.5rem;
}

.choice-item {
  margin: 0;
}

.choice-label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.choice-label:hover {
  border-color: var(--color-primary);
  background: rgba(124, 92, 255, 0.08);
  transform: translateX(4px);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
}

.choice-checkbox {
  width: 1.25rem;
  height: 1.25rem;
  accent-color: var(--color-primary);
}

.choice-text {
  flex: 1;
  color: var(--color-text);
  font-size: 1rem;
}

.validate-row {
  margin-top: 1rem;
}

.validate-row .btn {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  border-radius: var(--radius);
  border: none;
  font-weight: 600;
  cursor: pointer;
  background: var(--color-primary);
  color: white;
  transition: transform 0.2s ease, box-shadow 0.25s ease, filter 0.2s ease;
}

.validate-row .btn:hover:not(:disabled) {
  filter: brightness(1.1);
  transform: translateY(-2px);
  box-shadow: 0 6px 24px rgba(124, 92, 255, 0.4);
}

.validate-row .btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.choice-btn {
  width: 100%;
  padding: 0.75rem 1rem;
  text-align: left;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
  font-size: 1rem;
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.choice-btn:hover:not(:disabled) {
  border-color: var(--color-primary);
  background: rgba(124, 92, 255, 0.1);
  transform: translateX(4px);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
}

.choice-btn:disabled {
  opacity: 0.7;
  cursor: default;
}

.waiting-section {
  margin-bottom: 1.5rem;
}

.points-toast {
  position: fixed;
  bottom: 2rem;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.25rem;
  border-radius: var(--radius);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
  z-index: 50;
}

.points-toast.correct {
  border-color: var(--color-primary);
  background: rgba(124, 92, 255, 0.15);
}

.points-value {
  font-weight: 700;
  font-size: 1.25rem;
  color: var(--color-primary);
}

.points-toast:not(.correct) .points-value {
  color: var(--color-text-muted);
}

.rank-badge {
  font-size: 0.85rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 999px;
  background: var(--color-primary);
  color: white;
}

.points-toast-enter-active,
.points-toast-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.points-toast-enter-from,
.points-toast-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(10px);
}

/* ——— Écran fin de partie ——— */
.game-ended-section {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.75rem;
  margin-bottom: 1.5rem;
  text-align: center;
}

.game-ended-header {
  margin-bottom: 1.5rem;
}

.game-ended-title {
  font-size: 1.5rem;
  margin: 0 0 0.25rem;
  color: var(--color-text);
  font-weight: 700;
}

.game-ended-sub {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.my-result-card {
  display: inline-flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 0.5rem 1rem;
  padding: 1rem 1.5rem;
  margin-bottom: 1.5rem;
  border-radius: var(--radius);
  background: rgba(124, 92, 255, 0.12);
  border: 1px solid rgba(124, 92, 255, 0.4);
}

.my-result-card.rank-1 {
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.2), rgba(255, 152, 0, 0.15));
  border-color: rgba(255, 193, 7, 0.5);
}

.my-result-card.rank-2 {
  background: linear-gradient(135deg, rgba(158, 158, 158, 0.2), rgba(97, 97, 97, 0.15));
  border-color: rgba(158, 158, 158, 0.5);
}

.my-result-card.rank-3 {
  background: linear-gradient(135deg, rgba(205, 127, 50, 0.25), rgba(184, 115, 51, 0.2));
  border-color: rgba(205, 127, 50, 0.5);
}

.my-result-label {
  width: 100%;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.my-result-rank {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--color-primary);
}

.my-result-card.rank-1 .my-result-rank { color: #d4a017; }
.my-result-card.rank-2 .my-result-rank { color: #78909c; }
.my-result-card.rank-3 .my-result-rank { color: #b8860b; }

.my-result-points {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text);
}

.final-ranking {
  margin-bottom: 1.5rem;
  text-align: left;
}

.final-ranking h3 {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 0.75rem;
  color: var(--color-text-muted);
}

.final-ranking-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.final-ranking-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  margin-bottom: 0.35rem;
  border-radius: var(--radius);
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  transition: border-color 0.2s ease, background 0.2s ease;
}

.final-ranking-item.is-me {
  border-color: var(--color-primary);
  background: rgba(124, 92, 255, 0.12);
  font-weight: 600;
}

.final-ranking-item.podium {
  border-color: rgba(124, 92, 255, 0.35);
}

.final-rank {
  flex-shrink: 0;
  width: 1.75rem;
  height: 1.75rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  font-weight: 700;
  border-radius: 999px;
  background: var(--color-border);
  color: var(--color-text);
}

.final-ranking-item:nth-child(1) .final-rank { background: linear-gradient(135deg, #ffc107, #ff9800); color: #1a1a1a; }
.final-ranking-item:nth-child(2) .final-rank { background: linear-gradient(135deg, #9e9e9e, #616161); color: #fff; }
.final-ranking-item:nth-child(3) .final-rank { background: linear-gradient(135deg, #cd7f32, #b87333); color: #fff; }

.final-pseudo {
  flex: 1;
  font-size: 1rem;
}

.final-points {
  font-weight: 600;
  color: var(--color-primary);
  font-size: 0.95rem;
}

.game-ended-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  justify-content: center;
}

.game-ended-actions .btn {
  padding: 0.65rem 1.25rem;
  font-weight: 600;
  text-decoration: none;
  border-radius: var(--radius);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.game-ended-actions .btn-primary {
  background: var(--color-primary);
  color: white;
  border: none;
}

.game-ended-actions .btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 16px rgba(124, 92, 255, 0.4);
}

.game-ended-actions .btn-outline {
  background: transparent;
  color: var(--color-primary);
  border: 1px solid var(--color-primary);
}

.game-ended-actions .btn-outline:hover {
  background: rgba(124, 92, 255, 0.1);
  transform: translateY(-1px);
}

.game-ended-enter-active,
.game-ended-leave-active {
  transition: opacity 0.35s ease, transform 0.35s ease;
}

.game-ended-enter-from,
.game-ended-leave-to {
  opacity: 0;
  transform: translateY(12px);
}

.ranking {
  margin-top: 1.5rem;
}

.ranking h2 {
  font-size: 1rem;
  margin-bottom: 0.5rem;
}

.ranking ol {
  padding-left: 1.25rem;
  color: var(--color-text-muted);
}

.muted {
  color: var(--color-text-muted);
}

.error {
  color: var(--color-error);
}
</style>
