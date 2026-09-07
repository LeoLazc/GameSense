import { Session } from '../types'
import { Status } from '../../../components/Status'

export function Welcome({ username, session, onStart, onResume, pending, error }: { username: string; session: Session | null; onStart: () => void; onResume: () => void; pending: boolean; error: string }) {
  const hasResume = !!session
  const completed = session?.progress.answered === session?.progress.total

  return (
    <section className="welcome-layout">
      <div className="margin-copy">
        <span>REGISTRO DE REVISOR / {username.toUpperCase()}</span>
        <h1>Establece tu elegibilidad.</h1>
        <p>
          Cada cuenta dispone de un único intento de evaluación. Responde cada pregunta con tus
          propias palabras; el servicio evalúa la respuesta después del envío.
        </p>
      </div>
      <div className="welcome-panel">
        <div className="panel-heading">
          <span>PUERTA DE CONOCIMIENTO / {completed ? 'COMPLETA' : 'LISTA'}</span>
          <h2>
            {hasResume ? (completed ? 'Evaluación completada' : 'Evaluación registrada') : 'Un único intento de evaluación'}
          </h2>
        </div>
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
        {!hasResume && (
          <p className="attempt-note">Esta cuenta puede comenzar ahora su único intento de evaluación.</p>
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