import { buildWeeks, compareIso, dayNames, todayIso } from '../lib/date'

interface Props {
  checkIns: Set<string>
  createdOn: string
  color: string
  weeks?: number
  disabled?: boolean
  onToggle: (date: string) => void
}

export default function CheckInGrid({ checkIns, createdOn, color, weeks = 12, disabled = false, onToggle }: Props) {
  const today = todayIso()
  const rows = buildWeeks(today, weeks)

  function cellClass(date: string): string {
    const classes = ['cell']
    if (checkIns.has(date)) classes.push('cell-on')
    if (compareIso(date, today) > 0) classes.push('cell-future')
    if (date === today) classes.push('cell-today')
    if (compareIso(date, createdOn) < 0) classes.push('cell-before')
    return classes.join(' ')
  }

  return (
    <div className="grid-wrap">
      <div className="grid-labels">
        {dayNames().map((d) => (
          <span key={d}>{d}</span>
        ))}
      </div>
      <div className="grid">
        {rows.map((row, i) => (
          <div key={i} className="grid-row">
            {row.map((date) => {
              const on = checkIns.has(date)
              const locked = disabled || compareIso(date, today) > 0 || compareIso(date, createdOn) < 0
              return (
                <button
                  key={date}
                  type="button"
                  className={cellClass(date)}
                  style={on ? { backgroundColor: color, borderColor: color } : undefined}
                  title={date}
                  disabled={locked}
                  onClick={() => onToggle(date)}
                />
              )
            })}
          </div>
        ))}
      </div>
    </div>
  )
}
