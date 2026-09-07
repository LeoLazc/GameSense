import axios from 'axios'

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message) }
}

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')
const client = axios.create({ baseURL: API_BASE_URL })

export async function request<T>(path: string, init: RequestInit = {}, token?: string): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const requestHeaders: Record<string, string> = {}
  headers.forEach((value, key) => { requestHeaders[key] = value })

  try {
    const response = await client.request<T>({
      url: path,
      method: init.method,
      headers: requestHeaders,
      data: init.body,
    })

    return (response.data === '' ? null : response.data) as T
  } catch (error: unknown) {
    if (!axios.isAxiosError(error)) {
      throw new ApiError(0, 'No se pudo completar la solicitud.')
    }

    const status = error.response?.status ?? 0
    if (!error.response) {
      throw new ApiError(0, 'No se pudo conectar con el servicio de GameSense. Comprueba tu conexión e inténtalo de nuevo.')
    }

    const body: unknown = error.response.data
    const message = typeof body === 'object' && body && 'message' in body && typeof body.message === 'string'
      ? body.message : status === 401 ? 'Tu sesión ha caducado. Inicia sesión de nuevo.' : status === 409 ? 'Esta cuenta ya ha utilizado su único intento de evaluación. Vuelve al inicio de cualificación para ver el resultado registrado.' : 'No se pudo completar la solicitud.'
    throw new ApiError(status, message)
  }
}
