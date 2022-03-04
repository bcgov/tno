import { loadingBarReducer } from 'react-redux-loading-bar';

import { appSlice, contentSlice, jwtSlice, lookupSlice, navSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [appSlice.name]: appSlice.reducer,
  [jwtSlice.name]: jwtSlice.reducer,
  [navSlice.name]: navSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
};
