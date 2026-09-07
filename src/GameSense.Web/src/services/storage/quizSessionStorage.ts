import { Session } from '../../features/quiz/types'

const SESSION_KEY = 'gamesense.quiz-session'

export const quizSessionStorage = {
  read: (): Session | null => {
    try { return JSON.parse(localStorage.getItem(SESSION_KEY) || 'null') as Session | null } catch { return null }
  },
  save: (session: Session) => localStorage.setItem(SESSION_KEY, JSON.stringify(session)),
  clear: () => localStorage.removeItem(SESSION_KEY),
}
