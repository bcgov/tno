import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICategoryModel,
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  ISeriesModel,
  ITagModel,
  IUserFilter,
  IUserModel,
} from 'hooks/api-editor';

import { IAdminState } from './interfaces';

export const initialAdminState: IAdminState = {
  dataSources: [],
  mediaTypes: [],
  users: { page: 1, quantity: 10, items: [], total: 0 },
  categories: [],
  tags: [],
  actions: [],
  series: [],
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
    storeCategories(state: IAdminState, action: PayloadAction<ICategoryModel[]>) {
      state.categories = action.payload;
    },
    storeTags(state: IAdminState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeActions(state: IAdminState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeSeries(state: IAdminState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    storeUserFilter(state: IAdminState, action: PayloadAction<IUserFilter>) {
      state.userFilter = action.payload;
    },
  },
});

export const {
  storeDataSources: storeAdminDataSources,
  storeMediaTypes: storeAdminMediaTypes,
  storeUsers: storeAdminUsers,
  storeCategories: storeAdminCategories,
  storeTags: storeAdminTags,
  storeActions: storeAdminActions,
  storeSeries: storeAdminSeries,
  storeUserFilter: storeAdminUserFilter,
} = adminSlice.actions;
