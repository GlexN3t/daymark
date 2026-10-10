export interface User {
  id: number
  email: string
  displayName: string
}

export interface AuthResponse {
  token: string
  user: User
}

export interface Habit {
  id: number
  name: string
  note: string | null
  color: string
  archived: boolean
  createdOn: string
  currentStreak: number
  longestStreak: number
  totalCheckIns: number
  lastCheckIn: string | null
  checkedInToday: boolean
}

export interface CheckIn {
  date: string
  note: string | null
}

export interface HabitDetail extends Habit {
  checkIns: CheckIn[]
}

export interface HabitSummary {
  id: number
  name: string
  color: string
  currentStreak: number
  longestStreak: number
  totalCheckIns: number
  checkedInToday: boolean
}

export interface Summary {
  activeHabits: number
  checkedInToday: number
  bestCurrentStreak: number
  bestLongestStreak: number
  completionRate: number
  habits: HabitSummary[]
}

export interface DayCount {
  date: string
  count: number
}
