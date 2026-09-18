import { request } from '../../../services/httpClient'
import { GameDetails, GameReview, RecentGame } from '../types'

export const gamesApi = {
  getRecent: (limit = 10) => request<RecentGame[]>(`/api/games/recent?limit=${limit}`),
  getById: (id: number, page = 1, token?: string) => request<GameDetails>(`/api/games/${id}?page=${page}`, {}, token),
  createReview: (id: number, rating: number, content: string, token: string) => request<GameReview>(`/api/games/${id}/reviews`, { method: 'POST', body: JSON.stringify({ title: 'Reseña', content, rating }) }, token),
}
