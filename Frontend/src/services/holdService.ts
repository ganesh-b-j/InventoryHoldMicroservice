import { CreateHoldRequest, HoldResponse } from '../types';
import { api, createApiError } from './api';

export async function createHoldRequest(request: CreateHoldRequest): Promise<HoldResponse> {
  try {
    const response = await api.post<HoldResponse>('/api/holds', request);
    return response.data;
  } catch (error) {
    throw new Error(createApiError(error));
  }
}

export async function releaseHoldRequest(holdId: string): Promise<void> {
  try {
    await api.delete(`/api/holds/${holdId}`);
  } catch (error) {
    throw new Error(createApiError(error));
  }
}
