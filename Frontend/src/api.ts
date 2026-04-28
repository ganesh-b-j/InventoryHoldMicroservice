import axios, { AxiosError } from 'axios';
import { CreateHoldRequest, HoldResponse, InventoryItemResponse, InventorySnapshotResponse } from './types';

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const api = axios.create({ baseURL: baseUrl, headers: { 'Content-Type': 'application/json' } });

function extractError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.message ?? error.message;
  }
  return (error as Error)?.message ?? 'Unknown error';
}

export async function fetchInventory(): Promise<InventoryItemResponse[]> {
  try {
    const response = await api.get<InventorySnapshotResponse>('/api/inventory');
    return response.data.items;
  } catch (error) {
    throw new Error(extractError(error));
  }
}

export async function createHold(request: CreateHoldRequest): Promise<HoldResponse> {
  try {
    const response = await api.post<HoldResponse>('/api/holds', request);
    return response.data;
  } catch (error) {
    throw new Error(extractError(error));
  }
}

export async function fetchHold(holdId: string): Promise<HoldResponse> {
  try {
    const response = await api.get<HoldResponse>(`/api/holds/${holdId}`);
    return response.data;
  } catch (error) {
    throw new Error(extractError(error));
  }
}

export async function releaseHold(holdId: string): Promise<void> {
  try {
    await api.delete(`/api/holds/${holdId}`);
  } catch (error) {
    throw new Error(extractError(error));
  }
}
