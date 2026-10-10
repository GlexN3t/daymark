// Everything here works on yyyy-MM-dd strings in UTC, matching the API.
const DAY_NAMES = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const

export function todayIso(): string {
  return new Date().toISOString().slice(0, 10)
}

export function isoDaysAgo(days: number): string {
  return shiftIso(todayIso(), -days)
}

export function shiftIso(iso: string, days: number): string {
  const d = parseIso(iso)
  d.setUTCDate(d.getUTCDate() + days)
  return toIso(d)
}

export function compareIso(a: string, b: string): number {
  return a < b ? -1 : a > b ? 1 : 0
}

export function formatDay(iso: string): string {
  return new Intl.DateTimeFormat('en-GB', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    timeZone: 'UTC',
  }).format(parseIso(iso))
}

export function monthShort(monthIndex: number): string {
  return new Intl.DateTimeFormat('en-GB', { month: 'short' }).format(new Date(Date.UTC(2026, monthIndex, 1)))
}

export function dayNames(): readonly string[] {
  return DAY_NAMES
}

function toIso(d: Date): string {
  return d.toISOString().slice(0, 10)
}

function parseIso(iso: string): Date {
  return new Date(`${iso}T00:00:00Z`)
}

/**
 * Rows of 7 day-strings (Mon..Sun), aligned so the first row starts on a
 * Monday and the last cell of the last row is on/after `endIso`.
 */
export function buildWeeks(endIso: string, weeks: number): string[][] {
  const end = parseIso(endIso)
  const mondayOfEndWeek = shiftDate(end, -((end.getUTCDay() + 6) % 7))

  let cursor = shiftDate(mondayOfEndWeek, -(weeks - 1) * 7)
  const rows: string[][] = []

  for (let w = 0; w < weeks; w++) {
    const row: string[] = []
    for (let d = 0; d < 7; d++) {
      row.push(toIso(cursor))
      cursor = shiftDate(cursor, 1)
    }
    rows.push(row)
  }

  return rows
}

/**
 * Columns of 7 day-strings for the full-year heatmap: one column per week,
 * starting at the Monday on/before Jan 1st.
 */
export function buildYearColumns(year: number): string[][] {
  const jan1 = new Date(Date.UTC(year, 0, 1))
  let cursor = shiftDate(jan1, -((jan1.getUTCDay() + 6) % 7))
  const last = new Date(Date.UTC(year, 11, 31))

  const columns: string[][] = []
  while (cursor.getTime() <= last.getTime()) {
    const week: string[] = []
    for (let d = 0; d < 7; d++) {
      week.push(toIso(cursor))
      cursor = shiftDate(cursor, 1)
    }
    columns.push(week)
  }

  return columns
}

function shiftDate(d: Date, days: number): Date {
  const copy = new Date(d)
  copy.setUTCDate(copy.getUTCDate() + days)
  return copy
}
