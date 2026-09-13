import { request } from '../../../services/httpClient'
import { AnswerResponse, AnswerSubmission, QuizResult, Session } from '../types'

export const quizApi = {
  start: (token: string) => request<Session>('/api/quiz/sessions', { method: 'POST' }, token),
  submitAnswers: (token: string, sessionId: number, answers: AnswerSubmission[]) => request<AnswerResponse>(`/api/quiz/sessions/${sessionId}/answers`, { method: 'POST', body: JSON.stringify({ answers }) }, token),
  result: (token: string, sessionId: number) => request<QuizResult>(`/api/quiz/sessions/${sessionId}/result`, {}, token),
}
