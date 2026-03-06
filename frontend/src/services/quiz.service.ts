/**
 * Service Quiz – liste, détail, CRUD, top10, publish.
 */

import type { QuizSummary, QuizDetail } from '@/types'
import { http } from './api/http'

export const quizService = {
  getMyQuizzes(ownerId: string): Promise<QuizSummary[]> {
    return http.get<QuizSummary[]>(`/api/quizzes?ownerId=${encodeURIComponent(ownerId)}`)
  },

  getPublished(): Promise<QuizSummary[]> {
    return http.get<QuizSummary[]>('/api/quizzes/published')
  },

  getTop10(): Promise<QuizSummary[]> {
    return http.get<QuizSummary[]>('/api/quizzes/top10')
  },

  getById(id: string): Promise<QuizSummary> {
    return http.get<QuizSummary>(`/api/quizzes/${id}`)
  },

  getDetail(id: string): Promise<QuizDetail> {
    return http.get<QuizDetail>(`/api/quizzes/${id}/detail`)
  },

  create(title: string, description: string, ownerId: string): Promise<QuizSummary> {
    return http.post<QuizSummary>('/api/quizzes', { title, description, ownerId })
  },

  update(id: string, title: string, description: string): Promise<void> {
    return http.put<void>(`/api/quizzes/${id}`, { title, description })
  },

  publish(id: string): Promise<void> {
    return http.post<void>(`/api/quizzes/${id}/publish`)
  },

  delete(id: string): Promise<void> {
    return http.delete<void>(`/api/quizzes/${id}`)
  },
}
