const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5243';

export async function api(path, options = {}) {
  const token = localStorage.getItem('medcontrol.token');
  const response = await fetch(`${API_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    let message = 'Não foi possível concluir a operação.';
    try {
      const body = await response.json();
      message = body.error ?? message;
    } catch {
      message = response.statusText || message;
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
