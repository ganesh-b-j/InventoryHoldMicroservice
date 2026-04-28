export interface HoldItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateHoldRequest {
  customerId: string;
  items: HoldItemRequest[];
  durationMinutes?: number;
}

export interface HoldItemResponse {
  productId: string;
  productName: string;
  quantity: number;
}

export interface HoldResponse {
  holdId: string;
  customerId: string;
  items: HoldItemResponse[];
  createdAt: string;
  expiresAt: string;
  status: string;
}

export interface InventoryItemResponse {
  productId: string;
  productName: string;
  availableQuantity: number;
}

export interface InventorySnapshotResponse {
  items: InventoryItemResponse[];
}
