const API_BASE = (import.meta.env?.VITE_API_URL as string | undefined) ?? '/api'

export const TOKEN_KEY = 'daymark.token'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

function storedToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

interface ProblemBody {
  detail?: string
  title?: string
  errors?: Record<string, string[]>
}

function messageFrom(body: ProblemBody | null): string {
  if (!body) return 'Something went wrong.'
  if (typeof body.detail === 'string') return body.detail
  if (body.errors) {
    const first = Object.values(body.errors).flat()[0]
    if (typeof first === 'string') return first
  }
  if (typeof body.title === 'string') return body.title
  return 'Something went wrong.'
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Content-Type', 'application/json')

  const token = storedToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers })

  if (res.status === 401) {
    localStorage.removeItem(TOKEN_KEY)
    if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
      window.location.assign('/login')
    }
  }

  if (!res.ok) {
    const body = (await res.json().catch(() => null)) as ProblemBody | null
    throw new ApiError(res.status, messageFrom(body))
  }

  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}
