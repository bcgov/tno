import { loadingBarReducer } from 'react-redux-loading-bar';

import { adminSlice, appSlice, contentSlice, lookupSlice } from './slices';
import { workOrderSlice } from './slices/work-orders';

export const reducer = {
  loadingBar: loadingBarReducer,
  [appSlice.name]: appSlice.reducer,
  [adminSlice.name]: adminSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
  [workOrderSlice.name]: workOrderSlice.reducer,
};
