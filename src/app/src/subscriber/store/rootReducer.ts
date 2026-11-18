import { appSlice, contentSlice, lookupSlice, profileSlice, settingsSlice } from './slices';
import { reportsSlice } from './slices/reports';

export const reducer = {
  [appSlice.name]: appSlice.reducer,
  [profileSlice.name]: profileSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
  [settingsSlice.name]: settingsSlice.reducer,
  [reportsSlice.name]: reportsSlice.reducer,
};
