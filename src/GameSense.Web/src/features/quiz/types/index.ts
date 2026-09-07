export type Question = { id: number; categoryId: number; difficulty: number; questionText: string; questionWeight: number }
export type Progress = { answered: number; total: number }
export type Session = { id: number; startedAt: string; completedAt: string | null; status: string; finalScore: number | null; questions: Question[]; progress: Progress }
export type QuizResult = { id: number; startedAt: string; completedAt: string | null; status: string; finalScore: number | null; expertiseScore: number | null; answers: unknown[] }
export type AnswerResponse = { completed: boolean; progress: Progress; nextQuestion: Question | null; result: QuizResult | null }

export type QuizView = 'welcome' | 'question' | 'evaluating' | 'error' | 'result'

export function currentQuestion(session: Session) {
  return session.questions[session.progress.answered]
}

export function progressPercent(progress: Progress) {
  return progress.total ? (progress.answered / progress.total) * 100 : 0
}
