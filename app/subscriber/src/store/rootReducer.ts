import { loadingBarReducer } from 'react-redux-loading-bar';

import { jwtSlice, navSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [jwtSlice.name]: jwtSlice.reducer,
  [navSlice.name]: navSlice.reducer,
};
