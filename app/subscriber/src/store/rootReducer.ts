import { loadingBarReducer } from 'react-redux-loading-bar';

import { appSlice, contentSlice, lookupSlice, profileSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [appSlice.name]: appSlice.reducer,
  [profileSlice.name]: profileSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
};
