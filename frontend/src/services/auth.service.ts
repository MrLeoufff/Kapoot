/**
 * Service d’authentification – port d’entrée vers l’API auth.
 */

import type { User, AuthLoginResponse } from '@/types'
import { http, setAuthToken } from './api/http'

export const authService = {
  async register(email: string, password: string, pseudo: string): Promise<User> {
    const user = await http.post<User>('/api/auth/register', { email, password, pseudo })
    return user
  },

  async login(email: string, password: string): Promise<AuthLoginResponse> {
    const data = await http.post<AuthLoginResponse>('/api/auth/login', { email, password })
    setAuthToken(data.token)
    return data
  },

  logout(): void {
    setAuthToken(null)
  },
}
