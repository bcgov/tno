import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IUserInfoModel } from 'tno-core';

import { IAjaxRequest, IAppState, IErrorModel } from '.';

const defaultState: IAppState = {
  requests: [],
  showErrors: false,
  errors: [],
};

/**
 * The following is a shorthand method for creating a reducer with paired actions and action creators.
 * All functionality related to this concept is contained within this file.
 * See https://redux-toolkit.js.org/api/createslice for more details.
 */
export const appSlice = createSlice({
  name: 'app',
  initialState: defaultState,
  reducers: {
    storeUserInfo(state: IAppState, action: PayloadAction<IUserInfoModel | undefined>) {
      state.userInfo = action.payload;
    },
    addRequest(state: IAppState, action: PayloadAction<IAjaxRequest>) {
      state.requests.push(action.payload);
    },
    removeRequest(state: IAppState, action: PayloadAction<string>) {
      const index = state.requests.findIndex((r) => r.url === action.payload);
      state.requests.splice(index, 1);
    },
    clearRequests(state: IAppState) {
      state.requests = [];
    },
    addError(state: IAppState, action: PayloadAction<IErrorModel>) {
      state.errors.push(action.payload);
      state.showErrors = true;
    },
    removeError(state: IAppState, action: PayloadAction<IErrorModel>) {
      const index = state.errors.indexOf(action.payload);
      state.errors.splice(index, 1);
      state.showErrors = state.errors.length > 0;
    },
    clearErrors(state: IAppState) {
      state.errors = [];
      state.showErrors = false;
    },
  },
});

export const {
  storeUserInfo,
  addRequest,
  removeRequest,
  clearRequests,
  addError,
  removeError,
  clearErrors,
} = appSlice.actions;
