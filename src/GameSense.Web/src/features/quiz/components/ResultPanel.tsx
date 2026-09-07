import { QuizResult } from '../types'

export function ResultPanel({ result, onReturnHome }: { result: QuizResult; onReturnHome: () => void }) {
  const score = result.expertiseScore ?? result.finalScore
  const passed = score !== null && score >= 90

  return (
    <section className="result-layout">
      <div className={`result-mark ${passed ? 'pass' : 'fail'}`} aria-hidden="true">
        {passed ? 'APTO' : 'NO APTO'}
      </div>
      <div className="result-report">
        <span>INFORME DE EVALUACIÓN / FINAL</span>
        <h1>{passed ? 'Elegibilidad de revisión confirmada.' : 'Elegibilidad de revisión no alcanzada.'}</h1>
        <p>
          {passed
            ? 'Tu conocimiento demostrado alcanza el umbral de cualificación de GameSense. Este resultado queda registrado en tu cuenta.'
            : 'Tu conocimiento demostrado está por debajo del umbral de cualificación actual. Este resultado queda registrado en tu cuenta; la política de un único intento no permite otra evaluación.'}
        </p>
        <div className="score-line">
          <span>PUNTUACIÓN DE CONOCIMIENTO</span>
          <strong>{score === null ? '—' : score.toFixed(1)}</strong>
          <small>UMBRAL / 90.0</small>
        </div>
        <div className="report-rule" />
        <button className="primary-button" onClick={onReturnHome}>
          Volver al inicio de cualificación
        </button>
      </div>
    </section>
  )
}