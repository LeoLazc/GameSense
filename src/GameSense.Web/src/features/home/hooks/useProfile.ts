import { useEffect, useState } from 'react'
import { profileApi } from '../services/profileApi'
import { Profile } from '../types'

export function useProfile(token: string | undefined, userId: number | undefined) {
  const [profile, setProfile] = useState<Profile | null>(null)
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  useEffect(() => {
    if (!token || !userId) return
    let active = true
    setPending(true)
    setError('')
    profileApi
      .getProfile(token, userId)
      .then(next => {
        if (active) setProfile(next)
      })
      .catch(failure => {
        if (active) {
          setError(
            failure instanceof Error
              ? failure.message
              : 'No se pudo cargar el perfil. Inténtalo de nuevo.',
          )
        }
      })
      .finally(() => {
        if (active) setPending(false)
      })
    return () => {
      active = false
    }
  }, [token, userId])

  return { profile, error, pending }
}