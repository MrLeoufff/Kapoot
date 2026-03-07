/**
 * Service admin – liste utilisateurs/quiz, suppression, promotion admin.
 * Réservé aux utilisateurs avec isAdmin (JWT claim).
 * Le token est passé explicitement pour éviter les soucis de code-splitting (chunk Admin).
 */

import type { AdminUser, QuizSummary } from '@/types'
import { http } from './api/http'

function authHeaders(token: string | null | undefined): RequestInit {
  if (!token) return {}
  return { headers: { Authorization: `Bearer ${token}` } }
}

export const adminService = {
  getUsers(token: string | null | undefined): Promise<AdminUser[]> {
    return http.get<AdminUser[]>('/api/admin/users', authHeaders(token))
  },

  getQuizzes(token: string | null | undefined): Promise<QuizSummary[]> {
    return http.get<QuizSummary[]>('/api/admin/quizzes', authHeaders(token))
  },

  deleteUser(userId: string, token: string | null | undefined): Promise<void> {
    return http.delete<void>(`/api/admin/users/${userId}`, authHeaders(token))
  },

  setUserAdmin(userId: string, isAdmin: boolean, token: string | null | undefined): Promise<void> {
    return http.put<void>(`/api/admin/users/${userId}/admin`, { isAdmin }, authHeaders(token))
  },

  deleteQuiz(quizId: string, token: string | null | undefined): Promise<void> {
    return http.delete<void>(`/api/admin/quizzes/${quizId}`, authHeaders(token))
  },
}
