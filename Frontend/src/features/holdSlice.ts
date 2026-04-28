import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { CreateHoldRequest, HoldResponse } from '../types';
import { createHoldRequest, releaseHoldRequest } from '../services/holdService';

export const createHold = createAsyncThunk(
  'hold/createHold',
  async (request: CreateHoldRequest) => await createHoldRequest(request),
);

export const releaseHold = createAsyncThunk(
  'hold/releaseHold',
  async (holdId: string) => {
    await releaseHoldRequest(holdId);
    return holdId;
  },
);

type HoldState = {
  items: HoldResponse[];
  loading: boolean;
  error: string | null;
};

const initialState: HoldState = {
  items: [],
  loading: false,
  error: null,
};

const holdSlice = createSlice({
  name: 'hold',
  initialState,
  reducers: {
    setHolds(state, action: PayloadAction<HoldResponse[]>) {
      state.items = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(createHold.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createHold.fulfilled, (state, action) => {
        state.loading = false;
        state.items.unshift(action.payload);
      })
      .addCase(createHold.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to create hold.';
      })
      .addCase(releaseHold.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(releaseHold.fulfilled, (state, action) => {
        state.loading = false;
        state.items = state.items.filter((hold) => hold.holdId !== action.payload);
      })
      .addCase(releaseHold.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to release hold.';
      });
  },
});

export const { setHolds } = holdSlice.actions;
export default holdSlice.reducer;
