import { useEffect, useState } from 'react'
import { gamesApi } from '../services/gamesApi'
import { RecentGame } from '../types'

export function useRecentGames(enabled: boolean) {
  const [games, setGames] = useState<RecentGame[]>([])
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!enabled) return
    let active = true
    setPending(true)
    setError('')
    gamesApi.getRecent()
      .then(next => { if (active) setGames(next) })
      .catch(failure => {
        if (active) setError(failure instanceof Error ? failure.message : 'No se pudieron cargar los videojuegos.')
      })
      .finally(() => { if (active) setPending(false) })
    return () => { active = false }
  }, [enabled])

  return { games, pending, error }
}
