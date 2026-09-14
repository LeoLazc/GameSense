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
        <h1>GameSense</h1>
        <p>
          Comprueba tu conocimientos en videojuegos,
        </p>
        <p> luego puntúa y reseña.</p>
      </div>
      <form className="form-panel" onSubmit={submit}>
        <div className="panel-heading">
          <span>
            {mode === 'login' ? 'LOGIN' : 'REGISTRO'}
          </span>
          <h2>{mode === 'login' ? 'Inicia sesión para continuar' : 'Registrate y comienza tu aventura'}</h2>
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
          {mode === 'login' ? 'no tienes un usuario? Registrate aquí' : 'Ya tengo una cuenta'}
        </button>
      </form>
    </section>
  )
}
