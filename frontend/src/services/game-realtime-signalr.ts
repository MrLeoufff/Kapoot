/**
 * Implémentation temps réel via SignalR (backend C#).
 */

import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import type { IGameRealtime } from './game-realtime.types'
import { createGameHubConnection } from './game-hub.service'

export function createSignalRGameRealtime(): IGameRealtime {
  const connected = ref(false)
  const handlers = new Map<string, Set<(payload: unknown) => void>>()
  let conn: signalR.HubConnection | null = null
  let isHost = false

  function on(event: string, handler: (payload: unknown) => void): void {
    if (!handlers.has(event)) handlers.set(event, new Set())
    handlers.get(event)!.add(handler)
  }

  function bindHandlers(): void {
    if (!conn) return
    handlers.forEach((set, event) => {
      set.forEach((handler) => {
        conn!.on(event, handler)
      })
    })
  }

  async function connectHost(code: string): Promise<void> {
    isHost = true
    conn = createGameHubConnection(null)
    bindHandlers()
    await conn.start()
    await conn.invoke('JoinAsHost', code)
    connected.value = true
  }

  async function connectPlayer(code: string, pId: string): Promise<void> {
    isHost = false
    conn = createGameHubConnection(pId)
    bindHandlers()
    await conn.start()
    await conn.invoke('JoinAsPlayer', code, pId)
    connected.value = true
  }

  async function startGame(): Promise<void> {
    if (!conn || !isHost) throw new Error('Non autorisé')
    await conn.invoke('StartGame')
  }

  async function showQuestion(questionIndex: number): Promise<void> {
    if (!conn || !isHost) throw new Error('Non autorisé')
    await conn.invoke('ShowQuestion', questionIndex)
  }

  async function submitAnswer(questionId: string, choiceIds: string[]): Promise<void> {
    if (!conn || isHost) throw new Error('Joueur non connecté')
    await conn.invoke('SubmitAnswer', questionId, choiceIds)
  }

  async function endQuestion(questionId: string, explanation?: string | null): Promise<void> {
    if (!conn || !isHost) throw new Error('Non autorisé')
    await conn.invoke('EndQuestion', questionId, explanation ?? null)
  }

  async function endGame(): Promise<void> {
    if (!conn || !isHost) throw new Error('Non autorisé')
    await conn.invoke('EndGame')
  }

  async function disconnect(): Promise<void> {
    if (conn) {
      try {
        await conn.stop()
      } catch {
        // ignore
      }
      conn = null
    }
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
