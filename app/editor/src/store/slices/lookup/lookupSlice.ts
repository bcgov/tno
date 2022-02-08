import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IContentTypeModel, IMediaTypeModel, IUserModel } from 'hooks';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  contentTypes: [],
  mediaTypes: [],
  users: [],
};

export const lookupSlice = createSlice({
  name: 'lookup',
  initialState: initialLookupState,
  reducers: {
    storeContentTypes(state: ILookupState, action: PayloadAction<IContentTypeModel[]>) {
      state.contentTypes = action.payload;
    },
    storeMediaTypes(state: ILookupState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeUsers(state: ILookupState, action: PayloadAction<IUserModel[]>) {
      state.users = action.payload;
    },
  },
});

export const { storeContentTypes, storeMediaTypes, storeUsers } = lookupSlice.actions;
