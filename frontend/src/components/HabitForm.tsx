import { useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError, request } from '../api/client'
import type { Habit } from '../api/types'

const PRESET_COLORS = ['#1f6f5c', '#3b6ef5', '#8a4f9e', '#a4552b', '#b4432f', '#5c6b1f']

interface Props {
  initial?: Habit | null
  onSaved: () => void
  onClose: () => void
}

export default function HabitForm({ initial, onSaved, onClose }: Props) {
  const [name, setName] = useState(initial?.name ?? '')
  const [note, setNote] = useState(initial?.note ?? '')
  const [color, setColor] = useState(initial?.color ?? PRESET_COLORS[0])
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const editing = initial != null

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')

    if (name.trim().length === 0) {
      setError('Give the habit a name.')
      return
    }

    setSaving(true)
    try {
      if (initial) {
        await request(`/habits/${initial.id}`, {
          method: 'PUT',
          body: JSON.stringify({ name: name.trim(), note, color, archived: initial.archived }),
        })
      } else {
        await request('/habits', {
          method: 'POST',
          body: JSON.stringify({ name: name.trim(), note, color }),
        })
      }
      onSaved()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not save the habit.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-label={editing ? 'Edit habit' : 'New habit'}
        onClick={(e) => e.stopPropagation()}
      >
        <h2>{editing ? 'Edit habit' : 'New habit'}</h2>
        <form onSubmit={(e) => void handleSubmit(e)}>
          <label className="field">
            <span>Name</span>
            <input
              type="text"
              value={name}
              maxLength={80}
              placeholder="e.g. Read 20 pages"
              onChange={(e) => setName(e.target.value)}
              autoFocus
            />
          </label>
          <label className="field">
            <span>Note <em>(optional)</em></span>
            <textarea
              value={note}
              maxLength={500}
              rows={2}
              placeholder="Anything that helps you stick to it"
              onChange={(e) => setNote(e.target.value)}
            />
          </label>
          <div className="field">
            <span>Color</span>
            <div className="swatches">
              {PRESET_COLORS.map((c) => (
                <button
                  key={c}
                  type="button"
                  className={c === color ? 'swatch swatch-active' : 'swatch'}
                  style={{ backgroundColor: c }}
                  aria-label={`Use color ${c}`}
                  aria-pressed={c === color}
                  onClick={() => setColor(c)}
                />
              ))}
              <input
                type="color"
                className="swatch swatch-custom"
                value={color}
                title="Pick any color"
                onChange={(e) => setColor(e.target.value)}
              />
            </div>
          </div>
          {error && <p className="form-error">{error}</p>}
          <div className="modal-actions">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving…' : editing ? 'Save changes' : 'Create habit'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
