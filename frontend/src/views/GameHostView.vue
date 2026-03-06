<template>
  <div class="game-host">
    <div class="host-header">
      <RouterLink to="/dashboard" class="back">← Tableau de bord</RouterLink>
      <h1>Écran présentateur</h1>
    </div>
    <div class="code-block">
      <p class="label">Code de la partie</p>
      <p class="code">{{ code }}</p>
      <p class="muted">Donnez ce code aux joueurs pour qu’ils rejoignent.</p>
    </div>
    <section class="players-section">
      <h2>Joueurs connectés</h2>
      <p v-if="!connected" class="muted">Connexion au serveur en cours…</p>
      <p v-else-if="connectionError" class="error">{{ connectionError }}</p>
      <ul v-else-if="players.length" class="player-list">
        <li v-for="p in players" :key="p.id">{{ p.pseudo }}</li>
      </ul>
      <p v-else class="muted">Aucun joueur pour le moment.</p>
    </section>
    <section v-if="currentQuestionText" class="current-question-section">
      <h2>Question en cours</h2>
      <p class="question-text">{{ currentQuestionText }}</p>
      <div class="answers-received">
        <h3>Réponses reçues ({{ playersWhoAnswered.length }})</h3>
        <ul v-if="playersWhoAnswered.length" class="answered-list">
          <li v-for="p in playersWhoAnswered" :key="p.playerId">{{ p.pseudo }}</li>
        </ul>
        <p v-else class="muted">En attente des réponses…</p>
      </div>
      <div v-if="pointsEarnedLog.length" class="points-earned-log">
        <h3>Points marqués</h3>
        <ul class="points-log-list">
          <li v-for="(entry, i) in pointsEarnedLog" :key="i" class="points-log-item">
            <span class="pseudo">{{ entry.pseudo }}</span>
            <span class="points">+{{ entry.points }} pts</span>
            <span v-if="entry.rank === 1" class="rank-tag">1ère</span>
            <span v-else-if="entry.rank === 2" class="rank-tag">2e</span>
            <span v-else-if="entry.rank === 3" class="rank-tag">3e</span>
          </li>
        </ul>
      </div>
    </section>
    <section v-if="connected && questions.length && !gameEnded" class="actions-section">
      <h2>Contrôles</h2>
      <p v-if="gameStarted" class="status">Partie en cours</p>
      <div class="controls">
        <button
          v-if="!gameStarted"
          type="button"
          class="btn btn-primary"
          :disabled="starting"
          @click="startGame"
        >
          Démarrer la partie
        </button>
        <template v-else>
          <div v-if="currentQuestionIndex < questions.length" class="question-controls">
            <button
              type="button"
              class="btn btn-primary"
              :disabled="showingQuestion"
              @click="showQuestion"
            >
              Afficher question {{ currentQuestionIndex + 1 }}
            </button>
            <button
              v-if="currentQuestionId"
              type="button"
              class="btn btn-outline"
              :disabled="endingQuestion"
              @click="endQuestion"
            >
              Fin de la question
            </button>
          </div>
          <button
            type="button"
            class="btn btn-danger"
            :disabled="endingGame"
            @click="endGame"
          >
            Fin de la partie
          </button>
        </template>
      </div>
    </section>
    <section v-if="ranking.length > 0" class="ranking-section">
      <h2>Classement</h2>
      <ol class="ranking-list">
        <li v-for="(r, i) in ranking" :key="r.playerId" class="ranking-item">
          <span class="rank">{{ i + 1 }}</span>
          <span class="pseudo">{{ r.pseudo }}</span>
          <span class="points">{{ r.totalPoints }} pts</span>
        </li>
      </ol>
    </section>
    <section v-if="gameEnded" class="game-ended-section">
      <h2>Partie terminée</h2>
      <p class="muted">La partie est terminée. Les joueurs ont vu le classement final.</p>
    </section>
    <p v-else-if="connected && !questions.length && !loadingQuiz" class="muted">Chargement du quiz…</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import * as signalR from '@microsoft/signalr'
import { createGameHubConnection } from '@/services/game-hub.service'
import { gameSessionService } from '@/services/game-session.service'
import { quizService } from '@/services/quiz.service'
import type { QuestionDetail } from '@/types'

const route = useRoute()
const code = computed(() => String(route.params.code ?? '').toUpperCase())

const connection = ref<signalR.HubConnection | null>(null)
const connected = ref(false)
const connectionError = ref('')
const players = ref<{ id: string; pseudo: string }[]>([])
let playersInterval: ReturnType<typeof setInterval> | null = null

const questions = ref<QuestionDetail[]>([])
const loadingQuiz = ref(true)
const gameStarted = ref(false)
const currentQuestionIndex = ref(0)
const currentQuestionId = ref<string | null>(null)
const currentQuestionText = ref('')
const playersWhoAnswered = ref<{ playerId: string; pseudo: string }[]>([])
const starting = ref(false)
const showingQuestion = ref(false)
const endingQuestion = ref(false)
const endingGame = ref(false)
const gameEnded = ref(false)

interface RankingEntry {
  playerId: string
  totalPoints: number
  pseudo: string
  rank?: number
}
const ranking = ref<RankingEntry[]>([])

interface PointsEarnedEntry {
  pseudo: string
  points: number
  rank: number
}
const pointsEarnedLog = ref<PointsEarnedEntry[]>([])
const maxPointsLogEntries = 10

function normalizeRanking(payload: unknown): RankingEntry[] {
  if (!Array.isArray(payload)) return []
  return payload.map((item: unknown) => {
    const o = item as Record<string, unknown>
    return {
      playerId: String(o.playerId ?? o.PlayerId ?? ''),
      totalPoints: Number(o.totalPoints ?? o.TotalPoints ?? 0),
      pseudo: String(o.pseudo ?? o.Pseudo ?? ''),
      rank: o.rank !== undefined || o.Rank !== undefined ? Number(o.rank ?? o.Rank) : undefined,
    }
  })
}

async function loadQuizAndPlayers() {
  try {
    const session = await gameSessionService.getByCode(code.value)
    const detail = await quizService.getDetail(session.quizId)
    questions.value = detail.questions
    const list = await gameSessionService.getPlayersByCode(code.value)
    players.value = list
  } catch {
    questions.value = []
  } finally {
    loadingQuiz.value = false
  }
}

onMounted(async () => {
  connectionError.value = ''
  const conn = createGameHubConnection(null)
  connection.value = conn
  conn.on('GameEnded', () => {
    gameEnded.value = true
  })

  conn.on('Ranking', (payload: unknown) => {
    ranking.value = normalizeRanking(payload)
  })

  conn.on('PointsEarned', (payload: { pseudo?: string; Pseudo?: string; pointsEarned?: number; PointsEarned?: number; rank?: number; Rank?: number }) => {
    const pseudo = payload.pseudo ?? payload.Pseudo ?? ''
    const points = payload.pointsEarned ?? payload.PointsEarned ?? 0
    const rank = payload.rank ?? payload.Rank ?? 0
    pointsEarnedLog.value = [{ pseudo, points, rank }, ...pointsEarnedLog.value].slice(0, maxPointsLogEntries)
  })

  conn.on('PlayerAnswered', (payload: { playerId: string; pseudo: string }) => {
    const id = payload.playerId ?? (payload as unknown as { PlayerId?: string }).PlayerId
    const pseudo = payload.pseudo ?? (payload as unknown as { Pseudo?: string }).Pseudo ?? ''
    if (!id || playersWhoAnswered.value.some((p) => p.playerId === id)) return
    playersWhoAnswered.value = [...playersWhoAnswered.value, { playerId: id, pseudo }]
  })

  try {
    await conn.start()
    await conn.invoke('JoinAsHost', code.value)
    connected.value = true
    await loadQuizAndPlayers()
    playersInterval = setInterval(async () => {
      if (!connected.value) return
      try {
        players.value = await gameSessionService.getPlayersByCode(code.value)
      } catch {
        // ignore
      }
    }, 2000)
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Connexion impossible.'
    connected.value = false
  }
})

onUnmounted(async () => {
  if (playersInterval) {
    clearInterval(playersInterval)
    playersInterval = null
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

async function startGame() {
  if (!connection.value) return
  starting.value = true
  try {
    await connection.value.invoke('StartGame')
    gameStarted.value = true
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Erreur'
  } finally {
    starting.value = false
  }
}

async function showQuestion() {
  if (!connection.value || currentQuestionIndex.value >= questions.value.length) return
  showingQuestion.value = true
  playersWhoAnswered.value = []
  pointsEarnedLog.value = []
  try {
    const idx = currentQuestionIndex.value
    const q = questions.value[idx]
    currentQuestionId.value = q.id
    currentQuestionText.value = q.text
    await connection.value.invoke('ShowQuestion', idx)
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Erreur'
  } finally {
    showingQuestion.value = false
  }
}

async function endQuestion() {
  if (!connection.value || !currentQuestionId.value) return
  endingQuestion.value = true
  try {
    const q = questions.value.find((x) => x.id === currentQuestionId.value)
    await connection.value.invoke('EndQuestion', currentQuestionId.value, q?.explanation ?? null)
    currentQuestionIndex.value += 1
    currentQuestionId.value = null
    currentQuestionText.value = ''
    playersWhoAnswered.value = []
    pointsEarnedLog.value = []
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Erreur'
  } finally {
    endingQuestion.value = false
  }
}

async function endGame() {
  if (!connection.value) return
  endingGame.value = true
  connectionError.value = ''
  try {
    await connection.value.invoke('EndGame')
    gameEnded.value = true
  } catch (e) {
    connectionError.value = e instanceof Error ? e.message : 'Erreur lors de la fin de partie.'
  } finally {
    endingGame.value = false
  }
}
</script>

<style scoped>
.game-host {
  max-width: 640px;
}

.host-header {
  margin-bottom: 1.5rem;
}

.back {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.back:hover {
  color: var(--color-primary);
}

.host-header h1 {
  margin: 0.25rem 0 0;
  font-size: 1.5rem;
}

.code-block {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.5rem;
  margin-bottom: 2rem;
  text-align: center;
}

.code-block .label {
  margin: 0 0 0.25rem;
  font-size: 0.9rem;
  color: var(--color-text-muted);
}

.code-block .code {
  font-size: 2rem;
  font-weight: 700;
  letter-spacing: 0.2em;
  margin: 0 0 0.5rem;
}

.current-question-section {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.25rem;
  margin-bottom: 1.5rem;
}

.current-question-section h2 {
  font-size: 1rem;
  margin: 0 0 0.5rem;
  color: var(--color-text-muted);
}

.current-question-section .question-text {
  font-size: 1.2rem;
  font-weight: 600;
  margin: 0 0 1rem;
  line-height: 1.4;
}

.answers-received h3 {
  font-size: 0.9rem;
  margin: 0 0 0.5rem;
  color: var(--color-text-muted);
  font-weight: 500;
}

.answered-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.answered-list li {
  padding: 0.35rem 0.75rem;
  background: rgba(124, 92, 255, 0.2);
  border-radius: 999px;
  font-size: 0.9rem;
}

.points-earned-log {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
}

.points-earned-log h3 {
  font-size: 0.9rem;
  margin: 0 0 0.5rem;
  color: var(--color-text-muted);
  font-weight: 500;
}

.points-log-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.points-log-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.25rem 0;
  font-size: 0.9rem;
}

.points-log-item .pseudo {
  font-weight: 500;
  min-width: 6rem;
}

.points-log-item .points {
  color: var(--color-primary);
  font-weight: 600;
}

.points-log-item .rank-tag {
  font-size: 0.75rem;
  padding: 0.15rem 0.4rem;
  border-radius: 999px;
  background: var(--color-primary);
  color: white;
}

.players-section h2,
.actions-section h2 {
  font-size: 1.1rem;
  margin-bottom: 0.75rem;
}

.player-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.player-list li {
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--color-border);
}

.controls {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
}

.question-controls {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: var(--radius);
  font-weight: 500;
  border: none;
  cursor: pointer;
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--color-primary);
  color: var(--color-primary);
}

.btn-danger {
  background: rgba(239, 68, 68, 0.2);
  color: var(--color-error);
}

.status {
  margin-bottom: 0.5rem;
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.muted {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.error {
  color: var(--color-error);
  font-size: 0.9rem;
}

.ranking-section {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.25rem;
  margin-bottom: 1.5rem;
}

.ranking-section h2 {
  font-size: 1.1rem;
  margin: 0 0 1rem;
  color: var(--color-text-muted);
}

.ranking-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.ranking-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.6rem 0;
  border-bottom: 1px solid var(--color-border);
}

.ranking-item:last-child {
  border-bottom: none;
}

.ranking-item .rank {
  font-weight: 700;
  color: var(--color-primary);
  min-width: 1.5rem;
}

.ranking-item .pseudo {
  flex: 1;
  font-weight: 500;
}

.ranking-item .points {
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.game-ended-section {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background: rgba(34, 197, 94, 0.1);
  border: 1px solid var(--color-success);
  border-radius: var(--radius);
}

.game-ended-section h2 {
  margin: 0 0 0.5rem;
  color: var(--color-success);
}
</style>
