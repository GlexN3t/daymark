import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function BrandMark() {
  return (
    <svg className="brand-mark" viewBox="0 0 24 24" aria-hidden="true">
      <rect x="2" y="2" width="20" height="20" rx="6" fill="currentColor" />
      <path
        d="M7.5 12.5l3.2 3.2L16.8 9"
        fill="none"
        stroke="#fff"
        strokeWidth="2.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <div className="app">
      <header className="topbar">
        <div className="container topbar-inner">
          <Link to="/" className="brand">
            <BrandMark />
            <span>Daymark</span>
          </Link>
          <nav className="nav">
            {user ? (
              <>
                <NavLink to="/" end>
                  Today
                </NavLink>
                <NavLink to="/stats">Stats</NavLink>
                <span className="nav-user" title={user.email}>
                  {user.displayName}
                </span>
                <button type="button" className="btn btn-ghost" onClick={handleLogout}>
                  Sign out
                </button>
              </>
            ) : (
              <>
                <NavLink to="/login">Sign in</NavLink>
                <NavLink to="/register" className="btn btn-primary btn-sm">
                  Create account
                </NavLink>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="container">
        <Outlet />
      </main>
      <footer className="footer container">
        <span>Daymark — a small habit tracker.</span>
      </footer>
    </div>
  )
}
