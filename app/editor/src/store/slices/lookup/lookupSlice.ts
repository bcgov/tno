import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  actions: [],
  sourceActions: [],
  sourceMetrics: [],
  categories: [],
  contentTypes: [],
  licenses: [],
  mediaTypes: [],
  dataSources: [],
  series: [],
  tags: [],
  tonePools: [],
  users: [],
};

export const lookupSlice = createSlice({
  name: 'lookup',
  initialState: initialLookupState,
  reducers: {
    storeActions(state: ILookupState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeSourceActions(state: ILookupState, action: PayloadAction<ISourceActionModel[]>) {
      state.sourceActions = action.payload;
    },
    storeSourceMetrics(state: ILookupState, action: PayloadAction<ISourceMetricModel[]>) {
      state.sourceMetrics = action.payload;
    },
    storeCategories(state: ILookupState, action: PayloadAction<ICategoryModel[]>) {
      state.categories = action.payload;
    },
    storeContentTypes(state: ILookupState, action: PayloadAction<IContentTypeModel[]>) {
      state.contentTypes = action.payload;
    },
    storeLicenses(state: ILookupState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeMediaTypes(state: ILookupState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeDataSources(state: ILookupState, action: PayloadAction<IDataSourceModel[]>) {
      state.dataSources = action.payload;
    },
    storeSeries(state: ILookupState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeTags(state: ILookupState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeTonePools(state: ILookupState, action: PayloadAction<ITonePoolModel[]>) {
      state.tonePools = action.payload;
    },
    storeUsers(state: ILookupState, action: PayloadAction<IUserModel[]>) {
      state.users = action.payload;
    },
  },
});

export const {
  storeActions,
  storeSourceActions,
  storeSourceMetrics,
  storeCategories,
  storeContentTypes,
  storeLicenses,
  storeMediaTypes,
  storeDataSources,
  storeSeries,
  storeTags,
  storeTonePools,
  storeUsers,
} = lookupSlice.actions;
