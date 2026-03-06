/**
 * Service GameSession – création, récupération par code, rejoindre.
 */

import type { GameSessionSummary } from '@/types'
import { http } from './api/http'

export interface GameSessionByCode {
  id: string
  quizId: string
  code: string
  status: number
}

export interface SessionPlayer {
  id: string
  pseudo: string
}

export interface JoinPayload {
  code: string
  userId?: string | null
  pseudo: string
}

export interface JoinResult {
  playerId: string
  gameSessionId: string
  pseudo: string
}

export const gameSessionService = {
  create(quizId: string, hostId: string): Promise<GameSessionSummary> {
    return http.post<GameSessionSummary>('/api/gamesessions', { quizId, hostId })
  },

  getByCode(code: string): Promise<GameSessionByCode> {
    return http.get<GameSessionByCode>(`/api/gamesessions/by-code/${encodeURIComponent(code)}`)
  },

  getPlayersByCode(code: string): Promise<SessionPlayer[]> {
    return http.get<SessionPlayer[]>(`/api/gamesessions/by-code/${encodeURIComponent(code)}/players`)
  },

  join(payload: JoinPayload): Promise<JoinResult> {
    return http.post<JoinResult>('/api/gamesessions/join', payload)
  },
}
