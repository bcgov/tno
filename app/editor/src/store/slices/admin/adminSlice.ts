import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  IUserFilter,
  IUserModel,
} from 'hooks/api-editor';

import { IAdminState } from './interfaces';

export const initialAdminState: IAdminState = {
  dataSources: [],
  mediaTypes: [],
  users: { page: 1, quantity: 10, items: [], total: 0 },
  userFilter: {
    sort: [],
  },
};

export const adminSlice = createSlice({
  name: 'admin',
  initialState: initialAdminState,
  reducers: {
    storeDataSources(state: IAdminState, action: PayloadAction<IDataSourceModel[]>) {
      state.dataSources = action.payload;
    },
    storeMediaTypes(state: IAdminState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeUsers(state: IAdminState, action: PayloadAction<IPaged<IUserModel>>) {
      state.users = action.payload;
    },
    storeUserFilter(state: IAdminState, action: PayloadAction<IUserFilter>) {
      state.userFilter = action.payload;
    },
  },
});

export const {
  storeDataSources: storeAdminDataSources,
  storeMediaTypes: storeAdminMediaTypes,
  storeUsers: storeAdminUsers,
  storeUserFilter: storeAdminUserFilter,
} = adminSlice.actions;
