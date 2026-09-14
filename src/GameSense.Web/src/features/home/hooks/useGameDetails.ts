import { useEffect, useState } from 'react'
import { gamesApi } from '../services/gamesApi'
import { GameDetails } from '../types'

export function useGameDetails(gameId: number | null) {
  const [game, setGame] = useState<GameDetails | null>(null)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (gameId == null) return
    let active = true
    setGame(null)
    setPending(true)
    setError('')
    gamesApi.getById(gameId)
      .then(next => { if (active) setGame(next) })
      .catch(failure => {
        if (active) setError(failure instanceof Error ? failure.message : 'No se pudo cargar el videojuego.')
      })
      .finally(() => { if (active) setPending(false) })
    return () => { active = false }
  }, [gameId])

  return { game, pending, error }
}
