/**
 * Service admin – liste utilisateurs/quiz, suppression, promotion admin.
 * Réservé aux utilisateurs avec isAdmin (JWT claim).
 */

import type { AdminUser, QuizSummary } from '@/types'
import { http } from './api/http'

export const adminService = {
  getUsers(): Promise<AdminUser[]> {
    return http.get<AdminUser[]>('/api/admin/users')
  },

  getQuizzes(): Promise<QuizSummary[]> {
    return http.get<QuizSummary[]>('/api/admin/quizzes')
  },

  deleteUser(userId: string): Promise<void> {
    return http.delete<void>(`/api/admin/users/${userId}`)
  },

  setUserAdmin(userId: string, isAdmin: boolean): Promise<void> {
    return http.put<void>(`/api/admin/users/${userId}/admin`, { isAdmin })
  },

  deleteQuiz(quizId: string): Promise<void> {
    return http.delete<void>(`/api/admin/quizzes/${quizId}`)
  },
}
