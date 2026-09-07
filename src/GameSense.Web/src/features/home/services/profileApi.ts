import { request } from '../../../services/httpClient'
import { Profile } from '../types'

export const profileApi = {
  getProfile: (token: string, userId: number) => request<Profile>(`/api/users/${userId}`, {}, token),
}