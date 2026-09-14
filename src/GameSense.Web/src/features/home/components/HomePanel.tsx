import { RecentGamesCarousel } from './RecentGamesCarousel'
import { RecentGame } from '../types'

export function HomePanel({
  games,
  gamesPending,
  gamesError,
  onOpenGame,
}: {
  games: RecentGame[]
  gamesPending: boolean
  gamesError: string
  onOpenGame: (id: number) => void
}) {
  return (
    <section className="home-layout">
      <RecentGamesCarousel games={games} pending={gamesPending} error={gamesError} onOpenGame={onOpenGame} />
    </section>
  )
}
