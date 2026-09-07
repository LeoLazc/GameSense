export type AuthMode = 'login' | 'register'
export type AuthFormValues = { identifier: string; username: string; email: string; password: string }
export type AuthResponse = { userId: number; username: string; email: string; accessToken: string; expiresAt: string }
export type AuthState = AuthResponse | null
