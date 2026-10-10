import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, request } from '../api/client'
import type { Habit, Summary } from '../api/types'
import HabitForm from '../components/HabitForm'
import { formatDay, todayIso } from '../lib/date'

function streakLabel(streak: number): string {
  if (streak <= 0) return 'No streak yet'
  return `${streak} day${streak === 1 ? '' : 's'}`
}

export default function Dashboard() {
  const [habits, setHabits] = useState<Habit[] | null>(null)
  const [summary, setSummary] = useState<Summary | null>(null)
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [showArchived, setShowArchived] = useState(false)
  const [busyId, setBusyId] = useState<number | null>(null)

  const today = todayIso()

  const load = useCallback(async () => {
    try {
      const [list, stats] = await Promise.all([
        request<Habit[]>('/habits?includeArchived=true'),
        request<Summary>('/stats/summary'),
      ])
      setHabits(list)
      setSummary(stats)
      setError('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load your habits.')
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function toggleToday(habit: Habit) {
    setBusyId(habit.id)
    try {
      if (habit.checkedInToday) {
        await request(`/habits/${habit.id}/check-ins/${today}`, { method: 'DELETE' })
      } else {
        await request(`/habits/${habit.id}/check-ins`, {
          method: 'POST',
          body: JSON.stringify({ date: today }),
        })
      }
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not update the check-in.')
    } finally {
      setBusyId(null)
    }
  }

  async function setArchived(habit: Habit, archived: boolean) {
    try {
      await request(`/habits/${habit.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          name: habit.name,
          note: habit.note,
          color: habit.color,
          archived,
        }),
      })
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not update the habit.')
    }
  }

  async function removeHabit(habit: Habit) {
    if (!window.confirm(`Delete "${habit.name}" and all its check-ins?`)) return
    try {
      await request(`/habits/${habit.id}`, { method: 'DELETE' })
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not delete the habit.')
    }
  }

  const visible = (habits ?? []).filter((h) => showArchived || !h.archived)

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">{formatDay(today)}</p>
          <h1>Today</h1>
        </div>
        <button type="button" className="btn btn-primary" onClick={() => setShowForm(true)}>
          New habit
        </button>
      </div>

      {summary && summary.activeHabits > 0 && (
        <p className="muted progress-line">
          {summary.checkedInToday} of {summary.activeHabits} done today
          {summary.bestCurrentStreak > 0 && (
            <>
              {' · '}best streak running: <strong>{summary.bestCurrentStreak}</strong>
            </>
          )}
        </p>
      )}

      {error && <p className="alert alert-error">{error}</p>}

      {habits === null ? (
        <p className="muted">Loading…</p>
      ) : visible.length === 0 ? (
        <div className="card empty">
          <h2>{showArchived ? 'Nothing here' : 'No habits yet'}</h2>
          <p className="muted">
            {showArchived
              ? 'You have no habits at all. Create one to get started.'
              : 'Create your first habit and check it off before bed.'}
          </p>
          <button type="button" className="btn btn-primary" onClick={() => setShowForm(true)}>
            Create your first habit
          </button>
        </div>
      ) : (
        <ul className="habit-list">
          {visible.map((habit) => (
            <li
              key={habit.id}
              className={habit.archived ? 'habit-card archived' : 'habit-card'}
              style={{ borderLeftColor: habit.color }}
            >
              <button
                type="button"
                className={habit.checkedInToday ? 'check check-on' : 'check'}
                style={habit.checkedInToday ? { backgroundColor: habit.color, borderColor: habit.color } : undefined}
                disabled={busyId === habit.id || habit.archived}
                onClick={() => void toggleToday(habit)}
                aria-label={habit.checkedInToday ? `Uncheck ${habit.name}` : `Check ${habit.name}`}
                aria-pressed={habit.checkedInToday}
              >
                {habit.checkedInToday && (
                  <svg viewBox="0 0 16 16" aria-hidden="true">
                    <path
                      d="M3.5 8.5l3.2 3.2 6-7"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2.2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                  </svg>
                )}
              </button>
              <div className="habit-main">
                <Link to={`/habits/${habit.id}`} className="habit-name">
                  {habit.name}
                </Link>
                <p className="habit-meta">
                  {streakLabel(habit.currentStreak)}
                  {habit.longestStreak > habit.currentStreak && ` · best ${habit.longestStreak}`}
                  {habit.archived && ' · archived'}
                </p>
              </div>
              <div className="habit-actions">
                <button
                  type="button"
                  className="btn btn-ghost btn-sm"
                  onClick={() => void setArchived(habit, !habit.archived)}
                >
                  {habit.archived ? 'Unarchive' : 'Archive'}
                </button>
                <button type="button" className="btn btn-ghost btn-sm btn-danger" onClick={() => void removeHabit(habit)}>
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {(habits ?? []).some((h) => h.archived) && (
        <label className="toggle-line">
          <input type="checkbox" checked={showArchived} onChange={(e) => setShowArchived(e.target.checked)} />
          Show archived habits
        </label>
      )}

      {showForm && (
        <HabitForm
          onClose={() => setShowForm(false)}
          onSaved={() => {
            setShowForm(false)
            void load()
          }}
        />
      )}
    </div>
  )
}
