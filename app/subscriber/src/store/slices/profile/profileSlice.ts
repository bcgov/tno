import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IReportResultForm } from 'features/my-reports/interfaces';
import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IReportModel,
  ISubscriberUserModel,
  ISystemMessageModel,
  ITonePoolModel,
  IUserColleagueModel,
} from 'tno-core';

import { IProfileState } from './interfaces';

export const initialProfileState: IProfileState = {
  contributors: [],
  myFilters: [],
  myFolders: [],
  myReports: [],
  myColleagues: [],
  reportsFilter: '',
  reportContent: {},
  messages: [],
  init: {
    myFilters: false,
    myFolders: false,
    myReports: false,
    myColleagues: false,
    myTonePool: false,
  },
  myTonePool: {
    ownerId: 0,
    isPublic: false,
    id: 0,
    name: '',
    description: '',
    sortOrder: 0,
    isEnabled: false,
  },
};

export const profileSlice = createSlice({
  name: 'profile',
  initialState: initialProfileState,
  reducers: {
    storeMyProfile(state: IProfileState, action: PayloadAction<ISubscriberUserModel | undefined>) {
      state.profile = action.payload;
    },
    storeImpersonate(
      state: IProfileState,
      action: PayloadAction<ISubscriberUserModel | undefined>,
    ) {
      state.impersonate = action.payload;
    },
    storeFilter(state: IProfileState, action: PayloadAction<IFilterModel | undefined>) {
      state.filter = action.payload;
    },
    storeMyFilters(state: IProfileState, action: PayloadAction<IFilterModel[]>) {
      state.myFilters = action.payload;
      state.init.myFilters = true;
    },
    storeMyFolders(state: IProfileState, action: PayloadAction<IFolderModel[]>) {
      state.myFolders = action.payload;
      state.init.myFolders = true;
    },
    storeMyReports(state: IProfileState, action: PayloadAction<IReportModel[]>) {
      state.myReports = action.payload;
      state.init.myReports = true;
    },
    storeMyColleagues(state: IProfileState, action: PayloadAction<IUserColleagueModel[]>) {
      state.myColleagues = action.payload;
      state.init.myColleagues = true;
    },
    storeReportsFilter(state: IProfileState, action: PayloadAction<string>) {
      state.reportsFilter = action.payload;
    },
    storeReportOutput(state: IProfileState, action: PayloadAction<IReportResultForm | undefined>) {
      state.reportOutput = action.payload;
    },
    storeReportContent(
      state: IProfileState,
      action: PayloadAction<{ [reportId: number]: number[] }>,
    ) {
      state.reportContent = action.payload;
    },
    contributors(state: IProfileState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeMyMessages(state: IProfileState, action: PayloadAction<ISystemMessageModel[]>) {
      state.messages = action.payload;
    },
    storeMyTonePool(state: IProfileState, action: PayloadAction<ITonePoolModel>) {
      state.myTonePool = action.payload;
      state.init.myTonePool = true;
    },
  },
});

export const {
  storeMyProfile,
  storeImpersonate,
  storeFilter,
  storeMyFilters,
  storeMyFolders,
  storeMyReports,
  storeMyColleagues,
  storeReportsFilter,
  storeReportOutput,
  storeReportContent,
  storeMyMessages,
  storeMyTonePool,
} = profileSlice.actions;
