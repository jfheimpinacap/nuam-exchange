const accessTokenStorageKey = 'nuam-exchange.access-token';

let accessToken: string | null = null;

function getSessionStorage(): Storage | null {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    return window.sessionStorage;
  } catch {
    return null;
  }
}

export function getAccessToken(): string | undefined {
  if (accessToken) {
    return accessToken;
  }

  const storedToken = getSessionStorage()?.getItem(accessTokenStorageKey)?.trim() ?? '';
  accessToken = storedToken || null;

  return accessToken ?? undefined;
}

export function setAccessToken(token: string): void {
  const normalizedToken = token.trim();

  if (!normalizedToken) {
    throw new Error('Access token must not be empty.');
  }

  accessToken = normalizedToken;
  getSessionStorage()?.setItem(accessTokenStorageKey, normalizedToken);
}

export function clearAccessToken(): void {
  accessToken = null;
  getSessionStorage()?.removeItem(accessTokenStorageKey);
}
