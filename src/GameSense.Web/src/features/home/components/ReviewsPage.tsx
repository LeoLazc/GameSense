import { Status } from '../../../components/Status'
import { GameDetails } from '../types'

export function ReviewsPage({
  game,
  pending,
  error,
  onBack,
  onPageChange,
}: {
  game: GameDetails | null
  pending: boolean
  error: string
  onBack: () => void
  onPageChange: (page: number) => void
}) {
  if (pending) return <section className="game-page-state"><p role="status">Cargando reseñas…</p></section>
  if (error) return <section className="game-page-state"><Status message={error} tone="error" /><button className="quiet-button" type="button" onClick={onBack}>Volver al videojuego</button></section>
  if (!game) return <section className="game-page-state"><h1>Videojuego no encontrado</h1><button className="quiet-button" type="button" onClick={onBack}>Volver al videojuego</button></section>

  return (
    <article className="reviews-page">
      <button type="button" className="back-link" onClick={onBack}>Volver al videojuego</button>
      <div className="section-heading">
        <h1>Reseñas de {game.name}</h1>
        <span>{game.averageScore == null ? 'Sin votos' : `${game.averageScore}/100 · basado en ${game.voteCount} votaciones`}</span>
      </div>
      {!game.reviews.length && <p className="empty-note">Todavía no hay reseñas para este videojuego.</p>}
      <div className="reviews-list">
        {game.reviews.map(review => (
          <article className="review-entry" key={review.id}>
            <div><strong>{review.username}</strong><span>{review.rating}/100</span></div>
            <p>{review.content}</p>
          </article>
        ))}
      </div>
      {game.totalPages > 1 && (
        <nav className="review-pagination" aria-label="Paginación de reseñas">
          <button type="button" disabled={game.currentPage === 1} onClick={() => onPageChange(game.currentPage - 1)}>Anterior</button>
          <span>Página {game.currentPage} de {game.totalPages}</span>
          <button type="button" disabled={game.currentPage === game.totalPages} onClick={() => onPageChange(game.currentPage + 1)}>Siguiente</button>
        </nav>
      )}
    </article>
  )
}