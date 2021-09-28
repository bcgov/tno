import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { IJwtState } from '.';

/**
 * The following is a shorthand method for creating a reducer with paired actions and action creators.
 * All functionality related to this concept is contained within this file.
 * See https://redux-toolkit.js.org/api/createslice for more details.
 */
export const jwtSlice = createSlice({
  name: 'jwt',
  initialState: {
    token: '',
  },
  reducers: {
    storeToken(state: IJwtState, action: PayloadAction<any>) {
      state.token = action.payload;
    },
  },
});

export const { storeToken } = jwtSlice.actions;
