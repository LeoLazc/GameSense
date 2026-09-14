import { QuizResult } from '../types'

export function ResultPanel({ result, onReturnHome }: { result: QuizResult; onReturnHome: () => void }) {
  const score = result.expertiseScore ?? result.finalScore
  const passed = score !== null && score >= 60

  return (
    <section className="result-layout">
      <div className={`result-mark ${passed ? 'pass' : 'fail'}`} aria-hidden="true">
        {passed ? 'APTO' : 'NO APTO'}
      </div>
      <div className="result-report">
        <span>INFORME DE EVALUACIÓN FINAL</span>
        <h1>{passed ? 'Felicitaciones! ahora puedes reseñar y puntuar videojuegos.' : 'Lamentamos que no hayas aprobado, pero puedes intentar nuevamente mañana.'}</h1>
        <p>
          {passed
            ? 'Admiramos tu conocimiento! Ahora puedes reseñar y puntuar videojuegos.'
            : 'Siempre hay lugar para mejorar. Puedes seguir explorando videojuegos.'}
        </p>
        <div className="score-line">
          <span>PUNTUACIÓN DE CONOCIMIENTO</span>
          <strong>{score === null ? '—' : score.toFixed(1)}</strong>
          <small>UMBRAL / 60.0</small>
        </div>
        <div className="report-rule" />
        <button className="primary-button" onClick={onReturnHome}>
          Volver al inicio del Quiz
        </button>
      </div>
    </section>
  )
}
