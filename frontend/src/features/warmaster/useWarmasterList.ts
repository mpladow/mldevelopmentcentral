import { useCallback, useState } from 'react';
import { apiBaseUrl } from '../../config/api';

export function useWarmasterList<T>(token: string, path: string) {
  const [items, setItems] = useState<T[]>([]);
  const [message, setMessage] = useState('');

  const loadItems = useCallback(async () => {
    setMessage('');

    const response = await fetch(`${apiBaseUrl}${path}`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      setMessage('Unable to load data.');
      setItems([]);
      return;
    }

    setItems((await response.json()) as T[]);
  }, [path, token]);

  return {
    items,
    message,
    loadItems,
  };
}
