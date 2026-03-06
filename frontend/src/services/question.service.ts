/**
 * Service Questions – ajout, modification, suppression (avec choix).
 */

import type { QuestionDetail, ChoiceDto } from '@/types'
import { http } from './api/http'

export interface ChoiceInput {
  text: string
  isCorrect: boolean
  order: number
}

export interface AddQuestionInput {
  text: string
  type: number
  explanation: string | null
  order: number
  choices: ChoiceInput[]
}

export interface UpdateQuestionInput {
  text: string
  explanation: string | null
  order: number
  choices: ChoiceInput[]
}

function toChoiceInput(c: ChoiceDto): ChoiceInput {
  return { text: c.text, isCorrect: c.isCorrect, order: c.order }
}

export const questionService = {
  add(quizId: string, payload: AddQuestionInput): Promise<void> {
    return http.post<void>(`/api/quizzes/${quizId}/questions`, payload)
  },

  update(questionId: string, payload: UpdateQuestionInput): Promise<void> {
    return http.put<void>(`/api/questions/${questionId}`, payload)
  },

  remove(questionId: string): Promise<void> {
    return http.delete<void>(`/api/questions/${questionId}`)
  },

  toUpdatePayload(question: QuestionDetail): UpdateQuestionInput {
    return {
      text: question.text,
      explanation: question.explanation ?? null,
      order: question.order,
      choices: question.choices.map(toChoiceInput),
    }
  },
}
