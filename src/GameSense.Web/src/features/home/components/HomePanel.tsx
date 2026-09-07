import { Session } from '../../quiz/types'

export function HomePanel({
  username,
  session,
  onStartQuiz,
  onProfile,
}: {
  username: string
  session: Session | null
  onStartQuiz: () => void
  onProfile: () => void
}) {
  const hasSession = !!session
  const completed = session?.progress.answered === session?.progress.total

  return (
    <section className="welcome-layout">
      <div className="margin-copy">
        <span>PANEL PRINCIPAL / {username.toUpperCase()}</span>
        <h1>Elige tu próximo paso.</h1>
        <p>
          GameSense verifica los conocimientos de los revisores antes de habilitar la creación de
          reseñas. La evaluación de conocimientos es el primer paso del proceso.
        </p>
      </div>
      <div className="welcome-panel">
        <div className="panel-heading">
          <span>PUERTA DE CONOCIMIENTO / {completed ? 'COMPLETA' : 'LISTA'}</span>
          <h2>Cualificación de revisor</h2>
        </div>
        {hasSession && (
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
        {!hasSession && (
          <p className="attempt-note">
            Cada cuenta dispone de un único intento de evaluación. Las respuestas esperadas
            permanecen privadas.
          </p>
        )}
        <button className="primary-button" onClick={onStartQuiz}>
          {hasSession
            ? completed
              ? 'Ver resultado'
              : 'Reanudar evaluación'
            : 'Comenzar evaluación'}
        </button>
        <button type="button" className="quiet-button" onClick={onProfile}>
          Ver mi perfil
        </button>
      </div>
    </section>
  )
}