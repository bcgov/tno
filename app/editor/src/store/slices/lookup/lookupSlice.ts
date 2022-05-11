import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IClaimModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  cache: [],
  actions: [],
  categories: [],
  claims: [],
  contentTypes: [],
  dataLocations: [],
  dataSources: [],
  licenses: [],
  mediaTypes: [],
  roles: [],
  series: [],
  sourceActions: [],
  sourceMetrics: [],
  tags: [],
  tonePools: [],
  users: [],
};

export const lookupSlice = createSlice({
  name: 'lookup',
  initialState: initialLookupState,
  reducers: {
    storeCache(state: ILookupState, action: PayloadAction<ICacheModel[]>) {
      state.cache = action.payload;
    },
    updateCache(state: ILookupState, action: PayloadAction<ICacheModel>) {
      let keyFound = false;
      state.cache = state.cache.map((c) => {
        if (c.key === action.payload.key) {
          keyFound = true;
          return { ...c, value: action.payload.value };
        }
        return c;
      });
      if (!keyFound) state.cache.push(action.payload);
    },
    storeActions(state: ILookupState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeCategories(state: ILookupState, action: PayloadAction<ICategoryModel[]>) {
      state.categories = action.payload;
    },
    storeClaims(state: ILookupState, action: PayloadAction<IClaimModel[]>) {
      state.claims = action.payload;
    },
    storeContentTypes(state: ILookupState, action: PayloadAction<IContentTypeModel[]>) {
      state.contentTypes = action.payload;
    },
    storeDataLocations(state: ILookupState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
    storeDataSources(state: ILookupState, action: PayloadAction<IDataSourceModel[]>) {
      state.dataSources = action.payload;
    },
    storeLicenses(state: ILookupState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeMediaTypes(state: ILookupState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeRoles(state: ILookupState, action: PayloadAction<IRoleModel[]>) {
      state.roles = action.payload;
    },
    storeSeries(state: ILookupState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeSourceActions(state: ILookupState, action: PayloadAction<ISourceActionModel[]>) {
      state.sourceActions = action.payload;
    },
    storeSourceMetrics(state: ILookupState, action: PayloadAction<ISourceMetricModel[]>) {
      state.sourceMetrics = action.payload;
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
  storeCache,
  updateCache,
  storeActions,
  storeCategories,
  storeClaims,
  storeContentTypes,
  storeDataLocations,
  storeDataSources,
  storeLicenses,
  storeMediaTypes,
  storeRoles,
  storeSeries,
  storeSourceActions,
  storeSourceMetrics,
  storeTags,
  storeTonePools,
  storeUsers,
} = lookupSlice.actions;
