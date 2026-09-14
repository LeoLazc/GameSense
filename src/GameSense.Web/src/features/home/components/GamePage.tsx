import { Status } from '../../../components/Status'
import { GameDetails } from '../types'

function formatReleaseDate(value: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES', { dateStyle: 'long', timeZone: 'UTC' }) : 'Fecha no disponible'
}

export function GamePage({ game, pending, error, onBack }: { game: GameDetails | null; pending: boolean; error: string; onBack: () => void }) {
  if (pending) return <section className="game-page-state"><p role="status">Cargando…</p></section>
  if (error) return <section className="game-page-state"><Status message={error} tone="error" /><button className="quiet-button" type="button" onClick={onBack}>Volver al inicio</button></section>
  if (!game) return <section className="game-page-state"><h1>Videojuego no encontrado</h1><button className="quiet-button" type="button" onClick={onBack}>Volver al inicio</button></section>

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
          <div className="section-heading"><h2 id="reviews-title">Reseñas de la comunidad</h2><span>{game.reviews.length}</span></div>
          {game.reviews.length === 0 && <p className="empty-note">Todavía no hay reseñas para este videojuego.</p>}
          <div className="reviews-list">{game.reviews.map(review => <article className="review-entry" key={review.id}><div><strong>{review.title}</strong><span>{review.username} · {review.rating}/5</span></div><p>{review.content}</p></article>)}</div>
        </section>
      </div>
    </article>
  )
}
