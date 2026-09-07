import { AuthPanel, useAuth } from './features/auth'
import { HomePanel, ProfilePanel, useHome, useProfile } from './features/home'
import { ErrorPanel, QuestionPanel, ResultPanel, useQuiz, Welcome } from './features/quiz'
import { quizSessionStorage } from './services/storage/quizSessionStorage'

function App() {
  const authController = useAuth()
  const quiz = useQuiz(authController.auth?.accessToken, authController.signOut)
  const auth = authController.auth
  const { surface, goHome, goProfile, goQuiz } = useHome(auth)
  const profileController = useProfile(auth?.accessToken, auth?.userId)

  const view = auth ? (surface === 'quiz' ? quiz.view : surface) : 'auth'

  const signOut = () => {
    quizSessionStorage.clear()
    authController.signOut()
    quiz.reset()
  }

  return (
    <div className="app-shell">
      <header className="instrument-bar">
        <a
          className="brand"
          href="/"
          onClick={event => {
            event.preventDefault()
            if (auth) goHome()
          }}
        >
          <span className="brand-mark" aria-hidden="true" />
          GameSense
        </a>
        <span className="bar-rule" />
        <span className="bar-context">CUALIFICACIÓN DE REVISORES / PUERTA DE CONOCIMIENTO</span>
        {auth && (
          <button
            className="user-button"
            onClick={goProfile}
            aria-label={`Abrir perfil de ${auth.username}`}
          >
            <span className="user-mark" aria-hidden="true" />
            {auth.username}
          </button>
        )}
      </header>
      <main className="console" aria-busy={authController.pending || quiz.pending || profileController.pending}>
        {view === 'auth' && (
          <AuthPanel
            onSubmit={authController.submit}
            pending={authController.pending}
            error={authController.error}
          />
        )}
        {view === 'home' && auth && (
          <HomePanel
            username={auth.username}
            session={quiz.session}
            onStartQuiz={goQuiz}
            onProfile={goProfile}
          />
        )}
        {view === 'profile' && auth && (
          <ProfilePanel
            username={auth.username}
            email={auth.email}
            userId={auth.userId}
            profile={profileController.profile}
            pending={profileController.pending}
            error={profileController.error}
            onSignOut={signOut}
            onHome={goHome}
          />
        )}
        {view === 'welcome' && auth && (
          <Welcome
            username={auth.username}
            session={quiz.session}
            onStart={quiz.begin}
            onResume={quiz.resume}
            pending={quiz.pending}
            error={quiz.error}
          />
        )}
        {(view === 'question' || view === 'evaluating') && quiz.session && (
          <QuestionPanel
            session={quiz.session}
            evaluating={view === 'evaluating'}
            onSubmit={quiz.submit}
          />
        )}
        {view === 'error' && (
          <ErrorPanel
            message={quiz.error}
            onRetry={quiz.session ? quiz.resume : quiz.begin}
            onSignOut={auth ? signOut : undefined}
          />
        )}
        {view === 'result' && quiz.result && (
          <ResultPanel result={quiz.result} onReturnHome={goHome} />
        )}
      </main>
      <footer className="footer">
        GAME SENSE <span>·</span> EVALUACIÓN CON RESPALDO DE API <span>·</span>{' '}
        {new Date().getFullYear()}
      </footer>
    </div>
  )
}

export default App