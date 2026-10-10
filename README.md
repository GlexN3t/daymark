# Daymark

![CI](https://github.com/GlexN3t/daymark/actions/workflows/ci.yml/badge.svg) ![CodeQL](https://github.com/GlexN3t/daymark/actions/workflows/codeql.yml/badge.svg) ![license: MIT](https://img.shields.io/badge/license-MIT-green) ![.NET 8](https://img.shields.io/badge/backend-.NET%208-512BD4) ![React 18](https://img.shields.io/badge/frontend-React%2018-61DAFB)

Daymark is a small habit tracker: check off something you did today, keep a streak
alive, and watch the year fill up. I built it to get comfortable with ASP.NET Core,
EF Core, JWT auth, and a React + TypeScript SPA — no UI framework, no auth library,
just the pieces wired together by hand.

Build and tests run on every push via the workflow in `.github/workflows/ci.yml`.

## Features

- Accounts with register / login (JWT, PBKDF2-hashed passwords)
- Habits with a name, note, color, and archive instead of delete-by-accident
- Daily check-ins — toggle today, backfill a missed day, or undo one
- Streaks (current + longest) computed on the server
- Stats page: done-today count, best streaks, 30-day completion rate, per-habit table
- Year calendar heatmap in the style of GitHub contributions
- Responsive layout, dark mode follows the OS setting

## Running it locally

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) and
[Node.js 20](https://nodejs.org/) (npm ships with it).

Terminal 1 — backend:

```bash
cd backend
dotnet run --project src/Daymark.Api
```

This starts the API on `http://localhost:5080` (Swagger UI at `/swagger`).
The first run creates `daymark.db` next to the project — SQLite, so there is
nothing to install or configure. In Development it also seeds a demo account:

- email: `demo@daymark.dev`
- password: `daymark-demo`

Terminal 2 — frontend:

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. During development Vite proxies `/api` to the
backend, so no extra config is needed.

Type-check and production-build the frontend (CI runs this too):

```bash
cd frontend
npm run build
```

Tests:

```bash
cd backend
dotnet test
```

## API

All habit and stats endpoints require `Authorization: Bearer <token>`.

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/auth/register` | Create an account, returns token + user |
| POST | `/api/auth/login` | Sign in, returns token + user |
| GET | `/api/auth/me` | Who the token belongs to |
| GET | `/api/habits?includeArchived=true` | Current user's habits with streaks |
| POST | `/api/habits` | Create a habit |
| GET | `/api/habits/{id}?from=&to=` | Habit detail with check-ins in range |
| PUT | `/api/habits/{id}` | Edit name / note / color / archived |
| DELETE | `/api/habits/{id}` | Delete a habit and its check-ins |
| POST | `/api/habits/{id}/check-ins` | Check in `{ "date": "2026-10-06" }` |
| DELETE | `/api/habits/{id}/check-ins/{date}` | Undo a check-in |
| GET | `/api/stats/summary` | Streaks and 30-day completion rate |
| GET | `/api/stats/calendar?year=2026` | Per-day check-in counts for the heatmap |

Validation errors come back as RFC 7807 problem details, so the frontend can
show the first message without guessing at shapes.

## Project structure

```text
daymark/
├── backend/
│   ├── Daymark.sln
│   ├── src/Daymark.Api/
│   │   ├── Controllers/     # thin: auth, habits, stats
│   │   ├── Data/            # EF Core context + dev seed data
│   │   ├── Domain/          # User, Habit, CheckIn
│   │   ├── Dtos/            # request/response shapes
│   │   └── Services/        # HabitService, StatsService, TokenService,
│   │                         # PasswordHasher, StreakCalculator
│   └── tests/Daymark.Tests/ # streak math, hashing, DTO mapping
└── frontend/
    └── src/
        ├── api/             # typed fetch client + DTO types
        ├── components/      # layout, habit form, calendars
        ├── context/         # auth state
        ├── lib/             # UTC date helpers
        ├── pages/           # login, register, dashboard, detail, stats
        └── styles/          # one hand-written stylesheet
```

## A few decisions worth mentioning

- **Dates are UTC, day-granular.** `today` on the server is
  `DateOnly.FromDateTime(DateTime.UtcNow)`, and the frontend sends the UTC date
  too, so a check-in means the same day on both sides.
- **Streaks don't reset at midnight.** If you haven't checked in yet today, the
  current streak counts back from yesterday. A habit you missed yesterday shows
  0 only when the gap is real.
- **Passwords use PBKDF2-SHA256** (210k iterations, random salt) with no extra
  dependency — the `PasswordHasher` is ~50 lines and fully tested.
- **No migrations (yet).** The schema is created with `EnsureCreated()` on
  startup, which keeps `git clone → dotnet run` working with zero setup.
  Before this ever touched real user data I'd switch to EF migrations.
- **Check-in delete is idempotent** and habit delete explicitly removes
  check-ins first, so nothing orphans even if SQLite has FK enforcement off.

## Security notes

- **Passwords:** PBKDF2-HMAC-SHA256, 210,000 iterations, 128-bit random salt
  per password. The iteration count is stored in the hash, so it can be raised
  later without breaking existing logins.
- **Auth endpoints** (`/api/auth/*`) are rate-limited to 10 requests/minute per
  IP to slow down password guessing.
- **JWTs** are validated for issuer, audience, lifetime, and signature
  (1-minute clock skew). The signing key in `appsettings.json` is a
  **development key** — the API refuses to start in Production with it. Set
  `Jwt__Key` (32+ characters) via environment variables when deploying.
- API responses carry `nosniff` / `DENY` framing / `no-referrer` headers; HSTS
  and a strict content policy apply outside Development. Swagger is
  Development-only.
- Check-ins for archived habits are rejected (409) — archiving freezes history.
- The frontend keeps the JWT in `localStorage`. That's the standard SPA
  tradeoff: simple, but any XSS would expose it. There is no
  `dangerouslySetInnerHTML` or inline handlers anywhere, and all rendering goes
  through React's escaping. For real production use I'd move to httpOnly cookies
  with CSRF tokens and add refresh-token rotation.
- Dependencies: `dotnet list package --vulnerable` for NuGet, Dependabot
  (`.github/dependabot.yml`) for weekly updates, and CodeQL analysis on push
  (`.github/workflows/codeql.yml`). Known `npm audit` findings triaged at
  release time: the esbuild dev-server notice is build-tooling only (never
  shipped to browsers), the vite `server.fs.deny` bypass (GHSA-fx2h-pf6j-xcff)
  targets the dev server on Windows only — this project is developed on Linux
  and ships static files, so neither applies — and the react-router
  open-redirect advisories require attacker-controlled navigation paths — every
  `Link`/`navigate` in this app uses hardcoded internal routes, and there is
  no SSR — so neither is exploitable here. The vite/react-router v7+ lines
  (which carry the remaining fixes) are left for Dependabot to propose as
  deliberate major upgrades.

## Roadmap
Things I'd do next, roughly in order:

1. EF Core migrations + a `docker-compose` Postgres option for production
2. Weekly habits and per-week targets (streak math gets interesting)
3. Reminders (email or push)
4. CSV export of a habit's history
5. Refresh tokens instead of a 7-day access token

## License

MIT — see [LICENSE](LICENSE).
