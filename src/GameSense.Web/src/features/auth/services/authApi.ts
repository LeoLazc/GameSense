import { request } from '../../../services/httpClient'
import { AuthResponse } from '../types'

export const authApi = {
  login: (identifier: string, password: string) => request<AuthResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify({ identifier, password }) }),
  register: (username: string, email: string, password: string) => request<AuthResponse>('/api/auth/register', { method: 'POST', body: JSON.stringify({ username, email, password }) }),
}
