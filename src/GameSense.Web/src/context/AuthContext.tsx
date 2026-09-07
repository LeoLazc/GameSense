import { createContext, ReactNode, useMemo, useState } from 'react'
import { authApi } from '../features/auth/services/authApi'
import { AuthFormValues, AuthMode, AuthState } from '../features/auth/types'
import { authStorage } from '../services/storage/authStorage'

type AuthContextValue = {
  auth: AuthState
  error: string
  pending: boolean
  submit: (mode: AuthMode, values: AuthFormValues) => Promise<void>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>(authStorage.read)
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  const value = useMemo<AuthContextValue>(() => ({
    auth,
    error,
    pending,
    submit: async (mode, values) => {
      setPending(true)
      setError('')
      try {
        const next = mode === 'login'
          ? await authApi.login(values.identifier, values.password)
          : await authApi.register(values.username, values.email, values.password)
        authStorage.save(next)
        setAuth(next)
      } catch (failure) {
        setError(failure instanceof Error ? failure.message : 'Falló la autenticación.')
      } finally {
        setPending(false)
      }
    },
    signOut: () => {
      authStorage.clear()
      setAuth(null)
      setError('')
    },
  }), [auth, error, pending])

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}
