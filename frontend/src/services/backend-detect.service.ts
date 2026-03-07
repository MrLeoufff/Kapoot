/**
 * Détection du backend (Symfony vs C#) pour activer Mercure ou SignalR.
 */

const getBaseUrl = (): string => import.meta.env.VITE_API_BASE_URL || ''

export type BackendType = 'symfony' | 'dotnet'

export interface BackendInfo {
  backend: BackendType
  mercureUrl?: string
}

export async function getBackendInfo(): Promise<BackendInfo> {
  const base = getBaseUrl()
  try {
    const res = await fetch(`${base}/api/game/mercure-url`)
    if (!res.ok) return { backend: 'dotnet' }
    const data = (await res.json()) as { backend?: string; mercureUrl?: string }
    if (data.backend === 'symfony' && typeof data.mercureUrl === 'string') {
      return { backend: 'symfony', mercureUrl: data.mercureUrl }
    }
  } catch {
    // réseau ou C# (pas de route /api/game/mercure-url)
  }
  return { backend: 'dotnet' }
}
