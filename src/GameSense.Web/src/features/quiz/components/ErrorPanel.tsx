export function ErrorPanel({ message, onRetry, onSignOut }: { message: string; onRetry: () => void; onSignOut?: () => void }) {
  return (
    <section className="error-panel">
      <span className="error-code">ATENCIÓN</span>
      <h1>No es posible continuar.</h1>
      <p role="alert">{message || 'El servicio no devolvió datos utilizables.'}</p>
      <button className="primary-button" onClick={onRetry}>
        Reintentar
      </button>
    </section>
  )
}