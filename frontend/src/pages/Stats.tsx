import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, request } from '../api/client'
import type { DayCount, Summary } from '../api/types'
import Heatmap from '../components/Heatmap'

export default function Stats() {
  const year = new Date().getUTCFullYear()

  const [summary, setSummary] = useState<Summary | null>(null)
  const [days, setDays] = useState<DayCount[] | null>(null)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      const [stats, calendar] = await Promise.all([
        request<Summary>('/stats/summary'),
        request<DayCount[]>(`/stats/calendar?year=${year}`),
      ])
      setSummary(stats)
      setDays(calendar)
      setError('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load stats.')
    }
  }, [year])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">The big picture</p>
          <h1>Stats</h1>
        </div>
      </div>

      {error && <p className="alert alert-error">{error}</p>}

      {!summary || !days ? (
        <p className="muted">Loading…</p>
      ) : summary.activeHabits === 0 ? (
        <div className="card empty">
          <h2>No data yet</h2>
          <p className="muted">
            <Link to="/">Create a habit</Link> and check it in a few times — stats will show up here.
          </p>
        </div>
      ) : (
        <>
          <div className="stat-row">
            <div className="card stat">
              <span className="stat-value">{summary.checkedInToday}/{summary.activeHabits}</span>
              <span className="stat-label">done today</span>
            </div>
            <div className="card stat">
              <span className="stat-value">{summary.bestCurrentStreak}</span>
              <span className="stat-label">best running streak</span>
            </div>
            <div className="card stat">
              <span className="stat-value">{summary.bestLongestStreak}</span>
              <span className="stat-label">longest ever</span>
            </div>
            <div className="card stat">
              <span className="stat-value">{summary.completionRate}%</span>
              <span className="stat-label">last 30 days</span>
            </div>
          </div>

          <section className="card section">
            <h2>{year} in check-ins</h2>
            <p className="muted">Every square is a day; darker means more habits done.</p>
            <Heatmap days={days} year={year} />
          </section>

          <section className="card section">
            <h2>Per habit</h2>
            <ul className="summary-list">
              {summary.habits.map((h) => (
                <li key={h.id}>
                  <Link to={`/habits/${h.id}`} className="summary-row">
                    <span className="dot" style={{ backgroundColor: h.color }} />
                    <span className="summary-name">{h.name}</span>
                    <span className="muted summary-numbers">
                      {h.currentStreak} running · {h.longestStreak} best · {h.totalCheckIns} total
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          </section>
        </>
      )}
    </div>
  )
}
