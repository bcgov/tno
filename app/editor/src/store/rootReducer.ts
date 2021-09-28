import { loadingBarReducer } from 'react-redux-loading-bar';

import { jwtSlice } from './slices';

export const reducer = {
  loadingBar: loadingBarReducer,
  [jwtSlice.name]: jwtSlice.reducer,
};
