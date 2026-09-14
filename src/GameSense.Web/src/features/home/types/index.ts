export type AppSurface = 'home' | 'profile' | 'game' | 'quiz'

export type RecentGame = {
  id: number
  name: string
  releaseYear: number
  description: string | null
  releasedAt: string | null
  coverImageUrl: string | null
  backgroundImageUrl: string | null
  websiteUrl: string | null
  franchiseName: string | null
}

export type GameReview = {
  id: number
  title: string
  content: string
  rating: number
  createdAt: string
  updatedAt: string
  username: string
}

export type GameDetails = RecentGame & {
  reviews: GameReview[]
}

export type Profile = {
  id: number
  username: string
  email: string
  createdAt: string
  expertiseScore: number | null
}
