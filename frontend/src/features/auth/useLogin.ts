import { useCallback, useState } from 'react';
import { apiBaseUrl } from '../../config/api';
import type { LoginResponse } from '../../types/auth';

export function useLogin() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const login = useCallback(async (email: string, password: string) => {
    setError('');
    setIsLoading(true);

    try {
      const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        throw new Error('The email or password is incorrect.');
      }

      return (await response.json()) as LoginResponse;
    } catch (error) {
      if (error instanceof Error) {
        setError(error.message);
        throw error;
      }

      const genericError = new Error('Unable to reach the API.');
      setError(genericError.message);
      throw genericError;
    } finally {
      setIsLoading(false);
    }
  }, []);

  return { login, isLoading, error };
}
