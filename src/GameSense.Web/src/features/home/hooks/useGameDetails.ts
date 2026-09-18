import { useCallback, useEffect, useState } from 'react'
import { gamesApi } from '../services/gamesApi'
import { GameDetails } from '../types'

export function useGameDetails(gameId: number | null, token?: string) {
  const [page, setPage] = useState(1)
  const [game, setGame] = useState<GameDetails | null>(null)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => { setPage(1) }, [gameId])

  useEffect(() => {
    if (gameId == null) return
    let active = true
    setGame(null)
    setPending(true)
    setError('')
    gamesApi.getById(gameId, page, token)
      .then(next => { if (active) setGame(next) })
      .catch(failure => {
        if (active) setError(failure instanceof Error ? failure.message : 'No se pudo cargar el videojuego.')
      })
      .finally(() => { if (active) setPending(false) })
    return () => { active = false }
  }, [gameId, page, token])

  const submitReview = useCallback(async (rating: number, content: string) => {
    if (gameId == null || !token) throw new Error('Debes iniciar sesión para votar.')
    await gamesApi.createReview(gameId, rating, content, token)
    const next = await gamesApi.getById(gameId, page, token)
    setGame(next)
  }, [gameId, page, token])

  return { game, pending, error, page, setPage, submitReview }
}
