import { useEffect, useState } from 'react'
import { AuthState } from '../../auth/types'
import { AppSurface } from '../types'

export function useHome(auth: AuthState) {
  const [surface, setSurface] = useState<AppSurface>('home')
  const [selectedGameId, setSelectedGameId] = useState<number | null>(null)

  // A different signed-in user (login, register, or sign-out) always lands on the hub.
  useEffect(() => {
    setSurface('home')
  }, [auth?.userId])

  return {
    surface,
    goHome: () => { setSelectedGameId(null); setSurface('home') },
    goProfile: () => setSurface('profile'),
    goQuiz: () => setSurface('quiz'),
    goGame: (gameId: number) => { setSelectedGameId(gameId); setSurface('game') },
    goReviews: () => setSurface('reviews'),
    selectedGameId,
  }
}
