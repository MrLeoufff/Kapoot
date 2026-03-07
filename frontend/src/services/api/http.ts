/**
 * Client HTTP de base – en-tête Authorization, base URL, gestion erreurs.
 * Couche infrastructure du front (équivalent “Infrastructure” en clean arch).
 */

const TOKEN_STORAGE_KEY = 'kapoot_token'

const getBaseUrl = (): string => {
  if (import.meta.env.VITE_API_BASE_URL) return import.meta.env.VITE_API_BASE_URL
  return ''
}

let authToken: string | null = null

/** Permet au store d'enregistrer une source de token (évite token non envoyé si init dans un ordre inattendu). */
let tokenGetter: (() => string | null) | null = null

/** Token lu à chaque requête (évite perte du token avec code-splitting / chunks). */
function getTokenForRequest(): string | null {
  if (typeof localStorage === 'undefined') return tokenGetter?.() ?? authToken
  return tokenGetter?.() ?? authToken ?? localStorage.getItem(TOKEN_STORAGE_KEY)
}

export function setAuthToken(token: string | null): void {
  authToken = token
}

export function setTokenGetter(getter: () => string | null): void {
  tokenGetter = getter
}

export function getAuthToken(): string | null {
  return authToken
}

export async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const url = path.startsWith('http') ? path : `${getBaseUrl()}${path}`
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }
  const token =
    (options.headers as Record<string, string> | undefined)?.Authorization?.replace(/^Bearer\s+/i, '') ??
    getTokenForRequest()
  const h = headers as Record<string, string>
  if (token) h['Authorization'] = `Bearer ${token}`

  const res = await fetch(url, { ...options, headers })
  const text = await res.text()
  let data: unknown = null
  if (text) {
    try {
      data = JSON.parse(text) as unknown
    } catch {
      // pas du JSON
    }
  }

  if (!res.ok) {
    const err = (data as { error?: string; message?: string }) ?? {}
    throw new Error(err.error ?? err.message ?? `HTTP ${res.status}`)
  }

  return data as T
}

export const http = {
  get: <T>(path: string, options?: RequestInit) => request<T>(path, { method: 'GET', ...options }),
  post: <T>(path: string, body?: unknown, options?: RequestInit) =>
    request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined, ...options }),
  put: <T>(path: string, body?: unknown, options?: RequestInit) =>
    request<T>(path, { method: 'PUT', body: body ? JSON.stringify(body) : undefined, ...options }),
  delete: <T>(path: string, options?: RequestInit) => request<T>(path, { method: 'DELETE', ...options }),
}
