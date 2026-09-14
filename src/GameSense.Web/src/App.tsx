import { AuthPanel, useAuth } from './features/auth'
import { GamePage, HomePanel, ProfilePanel, useGameDetails, useHome, useProfile, useRecentGames } from './features/home'
import { ErrorPanel, QuestionPanel, ResultPanel, useQuiz, Welcome } from './features/quiz'
import { quizSessionStorage } from './services/storage/quizSessionStorage'

function App() {
  const authController = useAuth()
  const quiz = useQuiz(authController.auth?.accessToken, authController.signOut)
  const auth = authController.auth
  const { surface, selectedGameId, goHome, goProfile, goQuiz, goGame } = useHome(auth)
  const profileController = useProfile(auth?.accessToken, auth?.userId)
  const gamesController = useRecentGames(!!auth)
  const gameController = useGameDetails(surface === 'game' ? selectedGameId : null)

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
            goHome()
          }}
        >
          <span className="brand-mark" aria-hidden="true" />
          GameSense
        </a>
         <span className="bar-rule" />
        {auth && (
          <>
            <button type="button" className="quiz-nav-button" onClick={goQuiz}>
              Quiz
            </button>
            <div className="user-menu">
              <button
                className="user-button"
                onClick={goProfile}
                aria-label={`Abrir perfil de ${auth.username}`}
                aria-haspopup="menu"
              >
                <span className="user-mark" aria-hidden="true" />
                {auth.username}
              </button>
              <div className="user-menu-popover" role="menu">
                <button type="button" role="menuitem" onClick={signOut}>Cerrar sesión</button>
              </div>
            </div>
          </>
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
            games={gamesController.games}
            gamesPending={gamesController.pending}
            gamesError={gamesController.error}
            onOpenGame={goGame}
          />
        )}
        {view === 'game' && auth && <GamePage game={gameController.game} pending={gameController.pending} error={gameController.error} onBack={goHome} />}
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
        GAME SENSE <span>··</span>{' '}
        {new Date().getFullYear()}
      </footer>
    </div>
  )
}

export default App
