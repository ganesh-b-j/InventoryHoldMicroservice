import { configureStore } from '@reduxjs/toolkit';
import inventoryReducer from './features/inventorySlice';
import holdReducer from './features/holdSlice';

export const store = configureStore({
  reducer: {
    inventory: inventoryReducer,
    hold: holdReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
