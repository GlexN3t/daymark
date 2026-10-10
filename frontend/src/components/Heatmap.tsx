import { Fragment } from 'react'
import { buildYearColumns, monthShort } from '../lib/date'
import type { DayCount } from '../api/types'

interface Props {
  days: DayCount[]
  year: number
}

function levelFor(count: number): number {
  if (count <= 0) return 0
  if (count === 1) return 1
  if (count === 2) return 2
  if (count === 3) return 3
  return 4
}

function labelForWeek(week: string[], index: number, year: number): string {
  if (index === 0) return ''
  const first = week.find((iso) => iso.startsWith(`${year}-`) && iso.slice(8) === '01')
  if (!first) return ''
  return monthShort(Number(first.slice(5, 7)) - 1)
}

export default function Heatmap({ days, year }: Props) {
  const counts = new Map(days.map((d) => [d.date, d.count]))
  const columns = buildYearColumns(year)

  return (
    <div className="heatmap-scroll">
      <div className="hm-grid">
        {columns.map((week, i) => (
          <Fragment key={i}>
            <span className="hm-label" style={{ gridColumn: i + 1, gridRow: 1 }}>
              {labelForWeek(week, i, year)}
            </span>
            {week.map((iso, d) => {
              const inYear = iso.startsWith(`${year}-`)
              const count = counts.get(iso) ?? 0
              return (
                <span
                  key={iso}
                  className={inYear ? `hm-cell hm-${levelFor(count)}` : 'hm-cell hm-out'}
                  style={{ gridColumn: i + 1, gridRow: d + 2 }}
                  title={inYear ? `${iso}: ${count} check-in${count === 1 ? '' : 's'}` : undefined}
                />
              )
            })}
          </Fragment>
        ))}
      </div>
      <div className="hm-legend">
        <span>Less</span>
        <span className="hm-cell hm-0" />
        <span className="hm-cell hm-1" />
        <span className="hm-cell hm-2" />
        <span className="hm-cell hm-3" />
        <span className="hm-cell hm-4" />
        <span>More</span>
      </div>
    </div>
  )
}
