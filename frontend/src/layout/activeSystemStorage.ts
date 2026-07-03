const activeSystemStorageKey = 'mldev-dashboard.active-system';

export function loadStoredActiveSystemKey() {
  return window.localStorage.getItem(activeSystemStorageKey) ?? '';
}

export function saveStoredActiveSystemKey(systemKey: string) {
  if (systemKey) {
    window.localStorage.setItem(activeSystemStorageKey, systemKey);
    return;
  }

  window.localStorage.removeItem(activeSystemStorageKey);
}
