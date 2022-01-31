import { loadingBarReducer } from 'react-redux-loading-bar';

import { contentSlice, jwtSlice, lookupSlice, navSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [jwtSlice.name]: jwtSlice.reducer,
  [navSlice.name]: navSlice.reducer,
  [contentSlice.name]: contentSlice.reducer,
  [lookupSlice.name]: lookupSlice.reducer,
};
