import { Session } from '../types'
import { Status } from '../../../components/Status'

export function Welcome({ username, session, onStart, onResume, pending, error }: { username: string; session: Session | null; onStart: () => void; onResume: () => void; pending: boolean; error: string }) {
  const hasResume = !!session
  const completed = session?.progress.answered === session?.progress.total

  return (
    <section className="welcome-layout">
      <div className="margin-copy">
        <h1>ESTÁS LISTO?.</h1>
        <p>
          Dispones de un único intento de evaluación. Responde cada pregunta con tus
          propias palabras y nosotros nos encargamos de evaluarte.
        </p>
      </div>
      <div className="welcome-panel">
        {hasResume && (
          <div className="resume-line">
            <span className="detent active" />
            <div>
              <strong>
                {session.progress.answered} de {session.progress.total} respuestas registradas
              </strong>
              <small>Iniciada el {new Date(session.startedAt).toLocaleDateString()}</small>
            </div>
          </div>
        )}
        {error && <Status message={error} tone="error" />}
        <button className="primary-button" onClick={hasResume ? onResume : onStart} disabled={pending}>
          {pending
            ? 'Abriendo evaluación…'
            : hasResume
              ? completed
                ? 'Ver resultado'
                : 'Reanudar evaluación'
              : 'Comenzar evaluación'}
        </button>
      </div>
    </section>
  )
}