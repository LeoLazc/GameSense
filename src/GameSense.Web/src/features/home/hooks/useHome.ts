import { useEffect, useState } from 'react'
import { AuthState } from '../../auth/types'
import { AppSurface } from '../types'

export function useHome(auth: AuthState) {
  const [surface, setSurface] = useState<AppSurface>('home')

  // A different signed-in user (login, register, or sign-out) always lands on the hub.
  useEffect(() => {
    setSurface('home')
  }, [auth?.userId])

  return {
    surface,
    goHome: () => setSurface('home'),
    goProfile: () => setSurface('profile'),
    goQuiz: () => setSurface('quiz'),
  }
}