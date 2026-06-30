let accessToken: string | null = null;

export function getAccessToken(): string | undefined {
  return accessToken ?? undefined;
}

export function setAccessToken(token: string): void {
  const normalizedToken = token.trim();

  if (!normalizedToken) {
    throw new Error('Access token must not be empty.');
  }

  accessToken = normalizedToken;
}

export function clearAccessToken(): void {
  accessToken = null;
}
