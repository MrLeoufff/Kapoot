/**
 * Factory : retourne l’implémentation temps réel selon le backend (Symfony → Mercure, C# → SignalR).
 */

import type { IGameRealtime } from './game-realtime.types'
import { getBackendInfo } from './backend-detect.service'
import { createMercureGameRealtime } from './game-realtime-mercure'
import { createSignalRGameRealtime } from './game-realtime-signalr'

export async function createGameRealtime(): Promise<IGameRealtime> {
  const { backend, mercureUrl } = await getBackendInfo()
  if (backend === 'symfony' && mercureUrl) {
    return createMercureGameRealtime(mercureUrl)
  }
  return createSignalRGameRealtime()
}

export type { IGameRealtime } from './game-realtime.types'
