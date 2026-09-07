import { AuthResponse } from '../../features/auth/types'

const AUTH_KEY = 'gamesense.auth'

export const authStorage = {
  read: (): AuthResponse | null => {
    try { return JSON.parse(localStorage.getItem(AUTH_KEY) || 'null') as AuthResponse | null } catch { return null }
  },
  save: (auth: AuthResponse) => localStorage.setItem(AUTH_KEY, JSON.stringify(auth)),
  clear: () => localStorage.removeItem(AUTH_KEY),
}
