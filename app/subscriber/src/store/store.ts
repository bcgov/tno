import { configureStore } from '@reduxjs/toolkit';
import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
import { loadingBarMiddleware } from 'react-redux-loading-bar';
import logger from 'redux-logger';

import { reducer } from './rootReducer';

export const store = configureStore({
  reducer: reducer,
  middleware: (getDefaultMiddleware: () => any[]) =>
    getDefaultMiddleware().concat(logger).concat(loadingBarMiddleware()),
  devTools: process.env.NODE_ENV !== 'production',
});
export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
