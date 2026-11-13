import { configureStore } from '@reduxjs/toolkit';
import { useDispatch, useSelector, type TypedUseSelectorHook } from 'react-redux';

import { reducer } from './rootReducer';

export type ActionDelegate<T> = T | ((values: T) => T);
export const store = configureStore({
  reducer,
  devTools: import.meta.env.MODE !== 'production',
});
export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
