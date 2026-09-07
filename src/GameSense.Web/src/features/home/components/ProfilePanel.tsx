import { Status } from '../../../components/Status'
import { Profile } from '../types'

export function ProfilePanel({
  username,
  email,
  userId,
  profile,
  pending,
  error,
  onSignOut,
  onHome,
}: {
  username: string
  email: string
  userId: number
  profile: Profile | null
  pending: boolean
  error: string
  onSignOut: () => void
  onHome: () => void
}) {
  return (
    <section className="profile-layout">
      <div className="margin-copy">
        <span>PERFIL DE REVISOR</span>
        <h1>Tu registro de revisor.</h1>
        <p>
          Los datos de tu cuenta identifican tus evaluaciones y reseñas dentro de GameSense.
        </p>
      </div>
      <div className="profile-panel">
        <div className="panel-heading">
          <span>IDENTIDAD / CUENTA</span>
          <h2>{username}</h2>
        </div>
        <dl className="profile-fields">
          <div className="profile-row">
            <dt>ID de usuario</dt>
            <dd>{userId}</dd>
          </div>
          <div className="profile-row">
            <dt>Nombre de usuario</dt>
            <dd>{username}</dd>
          </div>
          <div className="profile-row">
            <dt>Correo electrónico</dt>
            <dd>{email}</dd>
          </div>
          <div className="profile-row">
            <dt>Fecha de registro</dt>
            <dd>
              {profile?.createdAt
                ? new Date(profile.createdAt).toLocaleDateString()
                : pending
                  ? '…'
                  : '—'}
            </dd>
          </div>
          <div className="profile-row">
            <dt>Puntaje de conocimiento</dt>
            <dd>{profile?.expertiseScore != null ? profile.expertiseScore.toFixed(1) : '—'}</dd>
          </div>
        </dl>
        {pending && (
          <p className="profile-note" role="status" aria-live="polite">
            Cargando perfil…
          </p>
        )}
        {error && <Status message={error} tone="error" />}
        <button className="primary-button" onClick={onSignOut}>
          Cerrar sesión
        </button>
        <button type="button" className="quiet-button" onClick={onHome}>
          Volver al inicio
        </button>
      </div>
    </section>
  )
}