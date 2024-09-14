import { Action, ThunkAction, configureStore } from "@reduxjs/toolkit";
import flashbarReducer from "./flashbarSlice";

export const store = configureStore({
  reducer: {
    messages: flashbarReducer,
  },
});

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>;
// Inferred type: {items: FlashbarState}
export type AppDispatch = typeof store.dispatch;

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
