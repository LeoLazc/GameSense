import { request } from '../../../services/httpClient'
import { AnswerResponse, QuizResult, Session } from '../types'

export const quizApi = {
  start: (token: string) => request<Session>('/api/quiz/sessions', { method: 'POST' }, token),
  submitAnswer: (token: string, sessionId: number, questionId: number, answerText: string) => request<AnswerResponse>(`/api/quiz/sessions/${sessionId}/answers`, { method: 'POST', body: JSON.stringify({ questionId, answerText }) }, token),
  result: (token: string, sessionId: number) => request<QuizResult>(`/api/quiz/sessions/${sessionId}/result`, {}, token),
}
