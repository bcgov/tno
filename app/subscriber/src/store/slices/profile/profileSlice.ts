import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IMinisterModel,
  ISystemMessageModel,
  IUserModel,
} from 'tno-core';

import { IProfileState } from './interfaces';

export const initialProfileState: IProfileState = {
  contributors: [],
  myFilters: [],
  myFolders: [],
  myMinisters: [],
  systemMessages: [],
  sources: [],
};

export const profileSlice = createSlice({
  name: 'profile',
  initialState: initialProfileState,
  reducers: {
    storeMyProfile(state: IProfileState, action: PayloadAction<IUserModel | undefined>) {
      state.profile = action.payload;
    },
    storeMyFilters(state: IProfileState, action: PayloadAction<IFilterModel[]>) {
      state.myFilters = action.payload;
    },
    storeMyFolders(state: IProfileState, action: PayloadAction<IFolderModel[]>) {
      state.myFolders = action.payload;
    },
    storeMyMinisters(state: IProfileState, action: PayloadAction<IMinisterModel[]>) {
      state.myMinisters = action.payload;
    },
    contributors(state: IProfileState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeSystemMessages(state: IProfileState, action: PayloadAction<ISystemMessageModel[]>) {
      state.systemMessages = action.payload;
    },
  },
});

export const {
  storeMyProfile,
  storeMyFilters,
  storeMyFolders,
  storeMyMinisters,
  storeSystemMessages,
} = profileSlice.actions;
