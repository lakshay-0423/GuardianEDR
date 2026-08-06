const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';
const TOKEN_STORAGE_KEY = 'guardian-edr.tokens';
let refreshPromise;

export class ApiError extends Error {
  constructor(message, status, details) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

export const getStoredTokens = () => {
  const storedValue = localStorage.getItem(TOKEN_STORAGE_KEY);

  if (!storedValue) {
    return null;
  }

  try {
    return JSON.parse(storedValue);
  } catch {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    return null;
  }
};

export const storeTokens = (tokens) => {
  localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(tokens));
};

export const clearStoredTokens = () => {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
};

const parseResponse = async (response) => {
  const contentType = response.headers.get('content-type') ?? '';

  if (!contentType.includes('application/json')) {
    return null;
  }

  return response.json();
};

const sendRequest = async (path, options, accessToken) => {
  const headers = new Headers(options.headers);

  if (options.body) {
    headers.set('Content-Type', 'application/json');
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined,
  });
  const payload = await parseResponse(response);

  if (!response.ok) {
    throw new ApiError(
      payload?.message ?? 'The request could not be completed.',
      response.status,
      payload?.errors,
    );
  }

  return payload;
};

const refreshAccessToken = () => {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const tokens = getStoredTokens();

      if (!tokens?.refreshToken) {
        throw new ApiError('Your session has expired.', 401);
      }

      const payload = await sendRequest('/api/auth/refresh', {
        method: 'POST',
        body: { refreshToken: tokens.refreshToken },
      });

      storeTokens(payload.data.tokens);
      return payload.data.tokens.accessToken;
    })().finally(() => {
      refreshPromise = undefined;
    });
  }

  return refreshPromise;
};

export const apiRequest = async (
  path,
  options = {},
  { authenticate = true, retryOnUnauthorized = true } = {},
) => {
  const tokens = getStoredTokens();
  const accessToken = authenticate ? tokens?.accessToken : undefined;

  try {
    return await sendRequest(path, options, accessToken);
  } catch (error) {
    if (error instanceof ApiError && error.status === 401 && authenticate && retryOnUnauthorized) {
      try {
        const refreshedAccessToken = await refreshAccessToken();
        return await sendRequest(path, options, refreshedAccessToken);
      } catch (refreshError) {
        clearStoredTokens();
        throw refreshError;
      }
    }

    throw error;
  }
};
