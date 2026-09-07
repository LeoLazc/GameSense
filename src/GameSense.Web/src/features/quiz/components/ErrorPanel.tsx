export function ErrorPanel({ message, onRetry, onSignOut }: { message: string; onRetry: () => void; onSignOut?: () => void }) {
  return (
    <section className="error-panel">
      <span className="error-code">AVISO DEL SERVICIO</span>
      <h1>No pudimos completar ese paso.</h1>
      <p role="alert">{message || 'El servicio no devolvió datos utilizables.'}</p>
      <button className="primary-button" onClick={onRetry}>
        Reintentar
      </button>
      {onSignOut && (
        <button className="quiet-button" onClick={onSignOut}>
          Volver al inicio de sesión
        </button>
      )}
    </section>
  )
}