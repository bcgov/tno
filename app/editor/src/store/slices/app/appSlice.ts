import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IUserInfoModel } from 'hooks/api-editor';

import { IAppState } from '.';

/**
 * The following is a shorthand method for creating a reducer with paired actions and action creators.
 * All functionality related to this concept is contained within this file.
 * See https://redux-toolkit.js.org/api/createslice for more details.
 */
export const appSlice = createSlice({
  name: 'app',
  initialState: {
    requests: [],
  },
  reducers: {
    addRequest(state: IAppState, action: PayloadAction<string>) {
      state.requests.push(action.payload);
    },
    removeRequest(state: IAppState, action: PayloadAction<string>) {
      const index = state.requests.indexOf(action.payload);
      state.requests.splice(index, 1);
    },
    clearRequests(state: IAppState) {
      state.requests = [];
    },
    storeUserInfo(state: IAppState, action: PayloadAction<IUserInfoModel | undefined>) {
      state.userInfo = action.payload;
    },
    storeToken(state: IAppState, action: PayloadAction<any>) {
      state.token = action.payload;
    },
  },
});

export const { addRequest, removeRequest, clearRequests, storeUserInfo, storeToken } =
  appSlice.actions;
