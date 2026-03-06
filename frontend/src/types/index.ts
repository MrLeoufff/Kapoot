/**
 * Types partagés – alignés avec les DTOs du backend.
 */

export interface User {
  id: string
  email: string
  pseudo: string
  avatarUrl: string | null
  isAdmin?: boolean
}

export interface AdminUser {
  id: string
  email: string
  pseudo: string
  isAdmin: boolean
  dateCreated: string
}

export interface AuthLoginResponse {
  token: string
  user: User
}

export type QuizStatus = 0 | 1 // Draft | Published

export interface QuizSummary {
  id: string
  title: string
  description: string
  status: QuizStatus
  ownerId: string
}

export interface ChoiceDto {
  id: string
  text: string
  isCorrect: boolean
  order: number
}

export interface QuestionDetail {
  id: string
  text: string
  type: number
  explanation: string | null
  order: number
  choices: ChoiceDto[]
}

export interface QuizDetail {
  id: string
  title: string
  description: string
  status: QuizStatus
  ownerId: string
  questions: QuestionDetail[]
}

export interface GameSessionSummary {
  sessionId: string
  code: string
}

export interface ApiError {
  error?: string
  message?: string
}
