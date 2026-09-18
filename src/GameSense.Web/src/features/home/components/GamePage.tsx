import { FormEvent, useState } from 'react'
import { Status } from '../../../components/Status'
import { ApiError } from '../../../services/httpClient'
import { GameDetails } from '../types'

function formatReleaseDate(value: string | null) {
  return value
    ? new Date(value).toLocaleDateString('es-ES', { dateStyle: 'long', timeZone: 'UTC' })
    : 'Fecha no disponible'
}

export function GamePage({
  game,
  pending,
  error,
  onBack,
  onViewReviews,
  onSubmitReview,
}: {
  game: GameDetails | null
  pending: boolean
  error: string
  onBack: () => void
  onViewReviews: () => void
  onSubmitReview: (rating: number, content: string) => Promise<void>
}) {
  const [rating, setRating] = useState(50)
  const [content, setContent] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  if (pending) return <section className="game-page-state"><p role="status">Cargando…</p></section>
  if (error) return <section className="game-page-state"><Status message={error} tone="error" /><button className="quiet-button" type="button" onClick={onBack}>Volver al inicio</button></section>
  if (!game) return <section className="game-page-state"><h1>Videojuego no encontrado</h1><button className="quiet-button" type="button" onClick={onBack}>Volver al inicio</button></section>

  const locked = !game.isViewerEligible || game.hasViewerReviewed
  const lockMessage = game.hasViewerReviewed
    ? 'Ya votaste por este videojuego. Solo puedes votar una vez.'
    : 'Necesitas una puntuación de experiencia de al menos 60 para votar.'
  const scoreDescription = game.averageScore == null
    ? 'Sin votos'
    : `${game.averageScore}/100 · basado en ${game.voteCount} votaciones`

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitError('')
    setSubmitting(true)
    try {
      await onSubmitReview(rating, content)
      setContent('')
    } catch (failure) {
      setSubmitError(
        failure instanceof ApiError && failure.status === 403
          ? 'No tienes autorización para votar.'
          : failure instanceof ApiError && failure.status === 409
            ? 'Ya votaste por este videojuego.'
            : 'No se pudo enviar tu voto. Inténtalo nuevamente.',
      )
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <article className="game-page">
      <button type="button" className="back-link" onClick={onBack}>Volver a los últimos lanzamientos</button>
      <div className="game-page-photo">
        {game.backgroundImageUrl || game.coverImageUrl
          ? <img src={game.backgroundImageUrl || game.coverImageUrl || ''} alt={`Imagen de ${game.name}`} />
          : <span className="image-fallback">Imagen no disponible</span>}
      </div>
      <div className="game-page-information">
        <h1>{game.name}</h1>
        <dl className="game-meta detail-meta">
          <div><dt>Lanzamiento</dt><dd>{formatReleaseDate(game.releasedAt)}</dd></div>
          {game.franchiseName && <div><dt>Franquicia</dt><dd>{game.franchiseName}</dd></div>}
        </dl>
        <p className="game-description">{game.description || ''}</p>
        {game.websiteUrl && <a className="external-link" href={game.websiteUrl} target="_blank" rel="noreferrer">Sitio oficial</a>}

        <section className="reviews-section" aria-labelledby="reviews-title">
          <div className="section-heading">
            <h2 id="reviews-title">Reseñas de la comunidad</h2>
            <span>{scoreDescription}</span>
          </div>
          <a className="external-link reviews-link" href="#reviews" onClick={event => { event.preventDefault(); onViewReviews() }}>Ver Reviews</a>

          <form className="review-form" onSubmit={submit}>
            <h3>Tu voto</h3>
            <label title={locked ? lockMessage : undefined}>
              Puntuación: {rating}/100
              <input
                type="range"
                min="0"
                max="100"
                value={rating}
                disabled={locked || submitting}
                onChange={event => setRating(Number(event.target.value))}
                aria-describedby={locked ? 'review-lock' : 'review-warning'}
              />
            </label>
            {!locked && <p id="review-warning" className="review-note">Solo puedes votar una vez. Piénsalo cuidadosamente antes de enviar.</p>}
            <label title={locked ? lockMessage : undefined}>
              Reseña
              <textarea
                maxLength={500}
                value={content}
                disabled={locked || submitting}
                onChange={event => setContent(event.target.value)}
                aria-describedby={locked ? 'review-lock' : 'review-character-count'}
              />
              <span id="review-character-count" className="review-character-count">
                {content.length} / 500 caracteres
              </span>
            </label>
            {locked && <p id="review-lock" className="review-note" role="note">{lockMessage}</p>}
            {submitError && <Status message={submitError} tone="error" />}
            <button className="primary-button" type="submit" disabled={locked || submitting || !content.trim()}>{submitting ? 'Enviando…' : 'Enviar voto'}</button>
          </form>
        </section>
      </div>
    </article>
  )
}
