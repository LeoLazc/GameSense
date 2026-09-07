import { FormEvent, useState } from 'react'
import { Status } from '../../../components/Status'
import { AuthFormValues, AuthMode } from '../types'

export function AuthPanel({ onSubmit, pending, error }: { onSubmit: (mode: AuthMode, values: AuthFormValues) => void; pending: boolean; error: string }) {
  const [mode, setMode] = useState<AuthMode>('login')

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const data = new FormData(event.currentTarget)
    onSubmit(mode, {
      identifier: String(data.get('identifier') || ''),
      username: String(data.get('username') || ''),
      email: String(data.get('email') || ''),
      password: String(data.get('password') || ''),
    })
  }

  return (
    <section className="auth-layout">
      <div className="margin-copy">
        <span>PUNTO DE ACCESO</span>
        <h1>La cualificación comienza con un registro claro.</h1>
        <p>
          GameSense verifica los conocimientos del revisor antes de habilitar las reseñas. Tu
          evaluación queda vinculada a tu cuenta.
        </p>
      </div>
      <form className="form-panel" onSubmit={submit}>
        <div className="panel-heading">
          <span>
            IDENTIDAD / {mode === 'login' ? 'REVISOR EXISTENTE' : 'NUEVO REVISOR'}
          </span>
          <h2>{mode === 'login' ? 'Inicia sesión para continuar' : 'Crea tu registro de revisor'}</h2>
        </div>
        {mode === 'register' && (
          <>
            <label>
              Nombre de usuario
              <input name="username" required minLength={3} autoComplete="username" />
            </label>
            <label>
              Correo electrónico
              <input name="email" type="email" required autoComplete="email" />
            </label>
          </>
        )}
        {mode === 'login' && (
          <label>
            Nombre de usuario o correo electrónico
            <input name="identifier" required autoComplete="username" />
          </label>
        )}
        <label>
          Contraseña
          <input
            name="password"
            type="password"
            required
            minLength={8}
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
          />
        </label>
        {error && <Status message={error} tone="error" />}
        <button className="primary-button" disabled={pending}>
          {pending ? 'Verificando registro…' : mode === 'login' ? 'Iniciar sesión' : 'Registrarse'}
        </button>
        <button
          type="button"
          className="quiet-button"
          onClick={() => setMode(mode === 'login' ? 'register' : 'login')}
        >
          {mode === 'login' ? 'Crear un nuevo registro de revisor' : 'Ya tengo una cuenta'}
        </button>
      </form>
    </section>
  )
}
