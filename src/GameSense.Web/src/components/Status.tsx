export function Status({ message, tone }: { message: string; tone: 'error' }) {
  return (
    <div className={`status ${tone}`} role="alert">
      {message}
    </div>
  )
}
