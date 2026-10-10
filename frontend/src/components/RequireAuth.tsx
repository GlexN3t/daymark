import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function RequireAuth() {
  const { user, ready } = useAuth()

  if (!ready) return <div className="loading-full">Loading…</div>
  return user ? <Outlet /> : <Navigate to="/login" replace />
}
