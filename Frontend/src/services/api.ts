import axios from 'axios';

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export const api = axios.create({
  baseURL: baseUrl,
  headers: { 'Content-Type': 'application/json' },
});

export function createApiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.message ?? error.message;
  }

  return (error as Error)?.message ?? 'Unknown error';
}
