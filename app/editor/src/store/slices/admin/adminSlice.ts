import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IDataSourceModel } from 'hooks/api-editor';

import { IAdminState } from './interfaces';

export const initialAdminState: IAdminState = {
  dataSources: [],
};

export const adminSlice = createSlice({
  name: 'admin',
  initialState: initialAdminState,
  reducers: {
    storeDataSources(state: IAdminState, action: PayloadAction<IDataSourceModel[]>) {
      state.dataSources = action.payload;
    },
  },
});

export const { storeDataSources } = adminSlice.actions;
