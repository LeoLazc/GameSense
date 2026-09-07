import { useState } from 'react'
import { ApiError } from '../../../services/httpClient'
import { quizSessionStorage } from '../../../services/storage/quizSessionStorage'
import { QuizResult, Session } from '../types'
import { quizApi } from '../services/quizApi'
import { currentQuestion, QuizView } from '../types'

export function useQuiz(token: string | undefined, onUnauthorized: () => void) {
  const [session, setSession] = useState<Session | null>(quizSessionStorage.read)
  const [result, setResult] = useState<QuizResult | null>(null)
  const [view, setView] = useState<QuizView>('welcome')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  const reset = () => {
    quizSessionStorage.clear()
    setSession(null)
    setResult(null)
    setView('welcome')
    setError('')
  }

  const handleUnauthorized = () => {
    reset()
    onUnauthorized()
  }

  const begin = async () => {
    if (!token) return
    setPending(true)
    setError('')
    try {
      const next = await quizApi.start(token)
      quizSessionStorage.save(next)
      setSession(next)
      setView(next.questions.length ? 'question' : 'error')
      if (!next.questions.length) setError('Esta evaluación aún no tiene preguntas disponibles.')
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) handleUnauthorized()
      else setError(error instanceof Error ? error.message : 'No se pudo iniciar la evaluación.')
      setView('error')
    } finally {
      setPending(false)
    }
  }

  const loadResult = async () => {
    if (!token || !session) return
    setPending(true)
    setError('')
    try {
      const next = await quizApi.result(token, session.id)
      setResult(next)
      setView('result')
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) handleUnauthorized()
      else setError(error instanceof Error ? error.message : 'No se pudo cargar el resultado.')
      setView('error')
    } finally {
      setPending(false)
    }
  }

  const resume = () => {
    if (!session) return begin()
    if (session.progress.answered === session.progress.total) return void loadResult()
    if (session.questions.length) setView('question')
    else { setError('La evaluación guardada está vacía. Vuelve al inicio de cualificación o cierra sesión.'); setView('error') }
  }

  const submit = async (answerText: string) => {
    if (!token || !session) return
    setView('evaluating')
    setError('')
    try {
      const question = currentQuestion(session)
      const response = await quizApi.submitAnswer(token, session.id, question?.id || 0, answerText)
      const updated: Session = { ...session, progress: response.progress, completedAt: response.completed ? new Date().toISOString() : null, status: response.completed ? 'Completed' : session.status }
      quizSessionStorage.save(updated)
      setSession(updated)
      if (response.completed) {
        if (response.result) { setResult(response.result); setView('result') }
        else await loadResult()
      } else setView('question')
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) handleUnauthorized()
      else { setError(error instanceof Error ? error.message : 'No se pudo evaluar la respuesta.'); setView('error') }
    }
  }

  return { session, result, view, error, pending, begin, resume, submit, reset, returnHome: () => setView('welcome') }
}
