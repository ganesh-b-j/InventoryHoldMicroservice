import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { InventoryItemResponse } from '../types';
import { fetchInventoryItems } from '../services/inventoryService';

export const fetchInventory = createAsyncThunk(
  'inventory/fetchInventory',
  async () => await fetchInventoryItems(),
);

type InventoryState = {
  items: InventoryItemResponse[];
  loading: boolean;
  error: string | null;
};

const initialState: InventoryState = {
  items: [],
  loading: false,
  error: null,
};

const inventorySlice = createSlice({
  name: 'inventory',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchInventory.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchInventory.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchInventory.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to load inventory.';
      });
  },
});

export default inventorySlice.reducer;
