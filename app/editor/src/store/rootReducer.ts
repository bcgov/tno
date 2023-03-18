import { loadingBarReducer } from 'react-redux-loading-bar';

import { adminSlice, appSlice, contentSlice, lookupSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [appSlice.name]: appSlice.reducer,
  [adminSlice.name]: adminSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
};
