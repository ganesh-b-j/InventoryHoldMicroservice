import { InventorySnapshotResponse, InventoryItemResponse } from '../types';
import { api, createApiError } from './api';

export async function fetchInventoryItems(): Promise<InventoryItemResponse[]> {
  try {
    const response = await api.get<InventorySnapshotResponse>('/api/inventory');
    return response.data.items;
  } catch (error) {
    throw new Error(createApiError(error));
  }
}
