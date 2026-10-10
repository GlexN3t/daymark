import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import type { PropsWithChildren } from 'react'
import { TOKEN_KEY, request } from '../api/client'
import type { AuthResponse, User } from '../api/types'

interface AuthState {
  user: User | null
  ready: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, displayName: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<User | null>(null)
  const [ready, setReady] = useState(false)

  // If a token survived a page reload, check it's still valid before rendering.
  useEffect(() => {
    if (!localStorage.getItem(TOKEN_KEY)) {
      setReady(true)
      return
    }
    request<User>('/auth/me')
      .then(setUser)
      .catch(() => localStorage.removeItem(TOKEN_KEY))
      .finally(() => setReady(true))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const res = await request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    localStorage.setItem(TOKEN_KEY, res.token)
    setUser(res.user)
  }, [])

  const register = useCallback(async (email: string, displayName: string, password: string) => {
    const res = await request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, displayName, password }),
    })
    localStorage.setItem(TOKEN_KEY, res.token)
    setUser(res.user)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, ready, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
