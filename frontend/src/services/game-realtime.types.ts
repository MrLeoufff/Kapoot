/**
 * Interface commune pour le temps réel (SignalR ou Mercure).
 */

import type { Ref } from 'vue'

export interface IGameRealtime {
  /** Écouter un événement (GameStarted, ShowQuestion, PlayerAnswered, PointsEarned, Ranking, ShowResult, GameEnded) */
  on(event: string, handler: (payload: unknown) => void): void
  /** Connexion en tant qu’hôte (code partie). */
  connectHost(code: string): Promise<void>
  /** Connexion en tant que joueur (code partie + id joueur). */
  connectPlayer(code: string, playerId: string): Promise<void>
  /** Démarrer la partie (hôte). */
  startGame(): Promise<void>
  /** Afficher une question (hôte). */
  showQuestion(questionIndex: number): Promise<void>
  /** Envoyer la réponse (joueur). */
  submitAnswer(questionId: string, choiceIds: string[]): Promise<void>
  /** Clôturer la question (hôte). */
  endQuestion(questionId: string, explanation?: string | null): Promise<void>
  /** Terminer la partie (hôte). */
  endGame(): Promise<void>
  /** Déconnexion / fermeture. */
  disconnect(): Promise<void>
  /** Indicateur de connexion. */
  connected: Ref<boolean>
}
