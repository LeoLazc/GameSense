import { useState } from 'react'
import { Status } from '../../../components/Status'
import { RecentGame } from '../types'

function formatReleaseDate(value: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES', { dateStyle: 'long', timeZone: 'UTC' }) : 'Fecha no disponible'
}

export function RecentGamesCarousel({ games, pending, error, onOpenGame }: {
  games: RecentGame[]
  pending: boolean
  error: string
  onOpenGame: (id: number) => void
}) {
  const [index, setIndex] = useState(0)
  const game = games[index]
  const move = (amount: number) => setIndex(current => (current + amount + games.length) % games.length)

  return (
    <section className="recent-games" aria-labelledby="recent-games-title">
      <div className="section-heading">
        <div>
          <h2 id="recent-games-title">Últimos lanzamientos</h2>
        </div>
      {games.length > 0 && <span className="slide-count" aria-live="polite">{String(index + 1).padStart(2, '0')} / {String(games.length).padStart(2, '0')}</span>}
      </div>
      {pending && <p className="loading-note" role="status">Cargando lanzamientos…</p>}
      {error && <Status message={error} tone="error" />}
      {!pending && !error && games.length === 0 && <p className="empty-note">Todavía no hay lanzamientos disponibles.</p>}
      {game && (
        <div className="game-slide">
          <button className="game-image-button" type="button" onClick={() => onOpenGame(game.id)} aria-label={`Abrir ${game.name}`}>
            {game.backgroundImageUrl || game.coverImageUrl
              ? <img src={game.backgroundImageUrl || game.coverImageUrl || ''} alt={`Portada de ${game.name}`} />
              : <span className="image-fallback">Imagen no disponible</span>}
          </button>
          <div className="slide-information">
            <button type="button" className="game-title-link" onClick={() => onOpenGame(game.id)}>{game.name}</button>
            <p>{game.description || ''}</p>
            <dl className="game-meta">
              <div><dt>Lanzamiento</dt><dd>{formatReleaseDate(game.releasedAt)}</dd></div>
              {game.franchiseName && <div><dt>Franquicia</dt><dd>{game.franchiseName}</dd></div>}
              <div><dt>Puntuación</dt><dd>{game.voteCount ? `${game.averageScore}/100 · ${game.voteCount} votos` : 'Sin votos todavía'}</dd></div>
            </dl>
          </div>
        </div>
      )}
      {games.length > 1 && (
        <div className="carousel-controls">
          <button type="button" className="carousel-button" onClick={() => move(-1)} aria-label="Ver lanzamiento anterior">Anterior</button>
          <button type="button" className="carousel-button" onClick={() => move(1)} aria-label="Ver lanzamiento siguiente">Siguiente</button>
        </div>
      )}
    </section>
  )
}
