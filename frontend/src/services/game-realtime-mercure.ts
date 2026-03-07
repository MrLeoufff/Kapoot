/**
 * Implémentation temps réel via Mercure (backend Symfony).
 */

import { ref } from 'vue'
import type { IGameRealtime } from './game-realtime.types'
import { http } from './api/http'
import { gameSessionService } from './game-session.service'

export function createMercureGameRealtime(mercureUrl: string): IGameRealtime {
  const connected = ref(false)
  const handlers = new Map<string, Set<(payload: unknown) => void>>()
  let eventSource: EventSource | null = null
  let sessionId: string | null = null
  let currentPlayerId: string | null = null
  let role: 'host' | 'player' | null = null

  function on(event: string, handler: (payload: unknown) => void): void {
    if (!handlers.has(event)) handlers.set(event, new Set())
    handlers.get(event)!.add(handler)
  }

  function emit(event: string, payload: unknown): void {
    handlers.get(event)?.forEach((h) => h(payload))
  }

  async function connectHost(code: string): Promise<void> {
    const session = await gameSessionService.getByCode(code)
    sessionId = session.id
    role = 'host'
    const topic = `session/${sessionId}`
    const url = `${mercureUrl}${mercureUrl.includes('?') ? '&' : '?'}topic=${encodeURIComponent(topic)}`
    eventSource = new EventSource(url)
    eventSource.onmessage = (e) => {
      try {
        const data = JSON.parse(e.data) as { event?: string; ranking?: unknown[]; [k: string]: unknown }
        const ev = data.event as string
        if (!ev) return
        if ((ev === 'Ranking' || ev === 'GameEnded') && Array.isArray(data.ranking)) {
          emit(ev, data.ranking)
        } else {
          emit(ev, data)
        }
      } catch {
        // ignore invalid JSON
      }
    }
    eventSource.onerror = () => {
      connected.value = false
    }
    eventSource.onopen = () => {
      connected.value = true
    }
    connected.value = true
  }

  async function connectPlayer(code: string, playerId: string): Promise<void> {
    const session = await gameSessionService.getByCode(code)
    sessionId = session.id
    currentPlayerId = playerId
    role = 'player'
    const topic = `session/${sessionId}`
    const url = `${mercureUrl}${mercureUrl.includes('?') ? '&' : '?'}topic=${encodeURIComponent(topic)}`
    eventSource = new EventSource(url)
    eventSource.onmessage = (e) => {
      try {
        const data = JSON.parse(e.data) as { event?: string; ranking?: unknown[]; [k: string]: unknown }
        const ev = data.event as string
        if (!ev) return
        if ((ev === 'Ranking' || ev === 'GameEnded') && Array.isArray(data.ranking)) {
          emit(ev, data.ranking)
        } else {
          emit(ev, data)
        }
      } catch {
        // ignore
      }
    }
    eventSource.onerror = () => {
      connected.value = false
    }
    eventSource.onopen = () => {
      connected.value = true
    }
    connected.value = true
  }

  async function startGame(): Promise<void> {
    if (!sessionId) throw new Error('Session non connectée')
    await http.post('/api/game/start', { sessionId })
  }

  async function showQuestion(questionIndex: number): Promise<void> {
    if (!sessionId) throw new Error('Session non connectée')
    await http.post('/api/game/show-question', { sessionId, questionIndex })
  }

  async function submitAnswer(questionId: string, choiceIds: string[]): Promise<void> {
    if (!sessionId || !currentPlayerId || role !== 'player') throw new Error('Joueur non connecté')
    await http.post('/api/game/submit-answer', {
      sessionId,
      playerId: currentPlayerId,
      questionId,
      choiceIds,
    })
  }

  async function endQuestion(questionId: string, explanation?: string | null): Promise<void> {
    if (!sessionId) throw new Error('Session non connectée')
    await http.post('/api/game/end-question', { sessionId, questionId, explanation: explanation ?? null })
  }

  async function endGame(): Promise<void> {
    if (!sessionId) throw new Error('Session non connectée')
    await http.post('/api/game/end', { sessionId })
  }

  async function disconnect(): Promise<void> {
    if (eventSource) {
      eventSource.close()
      eventSource = null
    }
    sessionId = null
    currentPlayerId = null
    role = null
    connected.value = false
  }

  return {
    on,
    connectHost,
    connectPlayer,
    startGame,
    showQuestion,
    submitAnswer,
    endQuestion,
    endGame,
    disconnect,
    connected,
  }
}
