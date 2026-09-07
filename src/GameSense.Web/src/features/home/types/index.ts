export type AppSurface = 'home' | 'profile' | 'quiz'

export type Profile = {
  id: number
  username: string
  email: string
  createdAt: string
  expertiseScore: number | null
}