import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ApiError, request } from '../api/client'
import type { HabitDetail } from '../api/types'
import CheckInGrid from '../components/CheckInGrid'
import HabitForm from '../components/HabitForm'
import { formatDay, isoDaysAgo, todayIso } from '../lib/date'

export default function HabitDetail() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [habit, setHabit] = useState<HabitDetail | null>(null)
  const [error, setError] = useState('')
  const [editing, setEditing] = useState(false)

  const habitId = Number(id)

  const load = useCallback(async () => {
    if (!Number.isInteger(habitId)) {
      navigate('/', { replace: true })
      return
    }
    try {
      const detail = await request<HabitDetail>(
        `/habits/${habitId}?from=${isoDaysAgo(83)}&to=${todayIso()}`,
      )
      setHabit(detail)
      setError('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load the habit.')
    }
  }, [habitId, navigate])

  useEffect(() => {
    void load()
  }, [load])

  async function toggleDate(date: string) {
    if (!habit) return
    try {
      if (checkIns.has(date)) {
        await request(`/habits/${habit.id}/check-ins/${date}`, { method: 'DELETE' })
      } else {
        await request(`/habits/${habit.id}/check-ins`, {
          method: 'POST',
          body: JSON.stringify({ date }),
        })
      }
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not update the check-in.')
    }
  }

  async function toggleArchived() {
    if (!habit) return
    try {
      await request(`/habits/${habit.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          name: habit.name,
          note: habit.note,
          color: habit.color,
          archived: !habit.archived,
        }),
      })
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not update the habit.')
    }
  }

  async function removeHabit() {
    if (!habit) return
    if (!window.confirm(`Delete "${habit.name}" and all its check-ins?`)) return
    try {
      await request(`/habits/${habit.id}`, { method: 'DELETE' })
      navigate('/')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not delete the habit.')
    }
  }

  if (!habit && !error) return <p className="muted">Loading…</p>
  if (error && !habit) return <p className="alert alert-error">{error}</p>
  if (!habit) return null

  const checkIns = new Set(habit.checkIns.map((c) => c.date))

  return (
    <div className="page">
      <p className="crumbs">
        <Link to="/">Today</Link> <span aria-hidden="true">/</span> {habit.name}
      </p>

      <div className="page-head">
        <div className="title-row">
          <span className="dot" style={{ backgroundColor: habit.color }} />
          <h1>{habit.name}</h1>
        </div>
        <div className="page-actions">
          <button type="button" className="btn btn-ghost" onClick={() => setEditing(true)}>
            Edit
          </button>
          <button type="button" className="btn btn-ghost" onClick={() => void toggleArchived()}>
            {habit.archived ? 'Unarchive' : 'Archive'}
          </button>
          <button type="button" className="btn btn-ghost btn-danger" onClick={() => void removeHabit()}>
            Delete
          </button>
        </div>
      </div>

      {habit.note && <p className="habit-note">{habit.note}</p>}
      {error && <p className="alert alert-error">{error}</p>}

      <div className="stat-row">
        <div className="card stat">
          <span className="stat-value">{habit.currentStreak}</span>
          <span className="stat-label">current streak</span>
        </div>
        <div className="card stat">
          <span className="stat-value">{habit.longestStreak}</span>
          <span className="stat-label">longest streak</span>
        </div>
        <div className="card stat">
          <span className="stat-value">{habit.totalCheckIns}</span>
          <span className="stat-label">total check-ins</span>
        </div>
        <div className="card stat">
          <span className="stat-value">
            {habit.lastCheckIn ? formatDay(habit.lastCheckIn) : '—'}
          </span>
          <span className="stat-label">last checked in</span>
        </div>
      </div>

      <section className="card section">
        <h2>Last 12 weeks</h2>
        <p className="muted">Click a day to check it off or undo it.</p>
        <CheckInGrid
          checkIns={checkIns}
          createdOn={habit.createdOn}
          color={habit.color}
          disabled={habit.archived}
          onToggle={(date) => void toggleDate(date)}
        />
      </section>

      {editing && (
        <HabitForm
          initial={habit}
          onClose={() => setEditing(false)}
          onSaved={() => {
            setEditing(false)
            void load()
          }}
        />
      )}
    </div>
  )
}
