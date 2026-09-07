import { CSSProperties, FormEvent, useEffect, useState } from 'react'
import { Session } from '../types'
import { ErrorPanel } from './ErrorPanel'
import { currentQuestion, progressPercent } from '../types'

export function QuestionPanel({ session, evaluating, onSubmit }: { session: Session; evaluating: boolean; onSubmit: (answer: string) => void }) {
  const question = currentQuestion(session)
  const [answer, setAnswer] = useState('')
  const progress = progressPercent(session.progress)
  useEffect(() => setAnswer(''), [question?.id])
  if (!question) return <ErrorPanel message="No hay ninguna pregunta disponible para la evaluación actual." onRetry={() => undefined} />
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (answer.trim()) onSubmit(answer.trim())
  }

  return (
    <section className="quiz-layout">
      <aside className="progress-rail">
        <span>EVALUACIÓN / ACTIVA</span>
        <strong>
          {String(session.progress.answered + 1).padStart(2, '0')}{' '}
          <small>/ {String(session.progress.total).padStart(2, '0')}</small>
        </strong>
        <div className="rail-track">
          <i style={{ '--progress': `${progress}%` } as CSSProperties} />
        </div>
        <p>
          {session.progress.answered} respondidas
          <br />
          {session.progress.total - session.progress.answered} restantes
        </p>
      </aside>
      <form className="question-panel" onSubmit={submit}>
        <div className="question-meta">
          <span>PREGUNTA {String(session.progress.answered + 1).padStart(2, '0')}</span>
          <span>DIFICULTAD {question.difficulty}</span>
        </div>
        <h1>{question.questionText}</h1>
        <label className="answer-label" htmlFor="answer">
          Tu respuesta
        </label>
        <textarea
          id="answer"
          value={answer}
          onChange={event => setAnswer(event.target.value)}
          disabled={evaluating}
          autoFocus
          required
          rows={7}
          placeholder="Escribe una respuesta concisa y específica…"
        />
        {evaluating ? (
          <div className="evaluating" role="status" aria-live="polite">
            <span className="signal-pulse" />
            Evaluando respuesta
            <span className="loading-dots">...</span>
          </div>
        ) : (
          <button className="primary-button submit-button" disabled={!answer.trim()}>
            Enviar respuesta <span aria-hidden="true">→</span>
          </button>
        )}
        <p className="privacy-note">
          Las respuestas son evaluadas por el servicio de GameSense. Las respuestas esperadas
          permanecen privadas.
        </p>
      </form>
    </section>
  )
}