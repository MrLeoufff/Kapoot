/**
 * Store Pinia – état d’authentification (utilisateur courant, token).
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from '@/types'
import { authService } from '@/services/auth.service'
import { getAuthToken, setAuthToken } from '@/services/api/http'

const STORAGE_KEY_USER = 'kapoot_user'
const STORAGE_KEY_TOKEN = 'kapoot_token'

function loadStoredUser(): User | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY_USER)
    if (!raw) return null
    return JSON.parse(raw) as User
  } catch {
    return null
  }
}

function loadStoredToken(): string | null {
  return localStorage.getItem(STORAGE_KEY_TOKEN)
}

function saveUser(user: User | null): void {
  if (user) localStorage.setItem(STORAGE_KEY_USER, JSON.stringify(user))
  else localStorage.removeItem(STORAGE_KEY_USER)
}

function saveToken(token: string | null): void {
  if (token) localStorage.setItem(STORAGE_KEY_TOKEN, token)
  else localStorage.removeItem(STORAGE_KEY_TOKEN)
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(loadStoredUser())
  const storedToken = loadStoredToken()
  if (storedToken) setAuthToken(storedToken)
  const token = ref<string | null>(getAuthToken())

  const isAuthenticated = computed(() => !!user.value && !!token.value)

  function setUser(u: User | null): void {
    user.value = u
    saveUser(u)
  }

  function setToken(t: string | null): void {
    token.value = t
    setAuthToken(t)
    saveToken(t)
  }

  async function login(email: string, password: string): Promise<void> {
    const { token: t, user: u } = await authService.login(email, password)
    setToken(t)
    setUser(u)
  }

  async function register(email: string, password: string, pseudo: string): Promise<void> {
    await authService.register(email, password, pseudo)
    await login(email, password)
  }

  function logout(): void {
    authService.logout()
    setToken(null)
    setUser(null)
  }

  return {
    user,
    token,
    isAuthenticated,
    setUser,
    setToken,
    login,
    register,
    logout,
  }
})
