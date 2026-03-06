/**
 * Service Profil – données utilisateur connecté (GET /api/users/:userId/profile).
 */

import { http } from './api/http'

export interface Profile {
  userId: string
  pseudo: string
  avatarUrl: string | null
  gamesPlayed: number
  quizzesCreated: number
  quizzesHosted: number
}

export const profileService = {
  getProfile(userId: string): Promise<Profile> {
    return http.get<Profile>(`/api/users/${encodeURIComponent(userId)}/profile`)
  },
}
