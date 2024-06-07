import { adminSlice, appSlice, contentSlice, lookupSlice } from './slices';
import { settingsSlice } from './slices/settings';
import { workOrderSlice } from './slices/work-orders';

export const reducer = {
  [appSlice.name]: appSlice.reducer,
  [adminSlice.name]: adminSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
  [workOrderSlice.name]: workOrderSlice.reducer,
  [settingsSlice.name]: settingsSlice.reducer,
};
