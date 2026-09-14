import { request } from '../../../services/httpClient'
import { GameDetails, RecentGame } from '../types'

export const gamesApi = {
  getRecent: (limit = 10) => request<RecentGame[]>(`/api/games/recent?limit=${limit}`),
  getById: (id: number) => request<GameDetails>(`/api/games/${id}`),
}
