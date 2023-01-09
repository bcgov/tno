import { loadingBarReducer } from 'react-redux-loading-bar';

import { adminSlice, appSlice, contentSlice, lookupSlice, navSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [appSlice.name]: appSlice.reducer,
  [navSlice.name]: navSlice.reducer,
  [adminSlice.name]: adminSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
};
