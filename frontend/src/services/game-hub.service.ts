/**
 * Connexion SignalR au hub de jeu (présentateur et joueurs).
 */

import * as signalR from '@microsoft/signalr'

const getHubUrl = (): string => {
  const base = import.meta.env.VITE_API_BASE_URL || ''
  return `${base}/hubs/game`
}

export function createGameHubConnection(playerId?: string | null): signalR.HubConnection {
  let url = getHubUrl()
  if (playerId) {
    const sep = url.includes('?') ? '&' : '?'
    url += `${sep}playerId=${encodeURIComponent(playerId)}`
  }
  return new signalR.HubConnectionBuilder()
    .withUrl(url)
    .withAutomaticReconnect()
    .build()
}

export type GameHubConnection = signalR.HubConnection
