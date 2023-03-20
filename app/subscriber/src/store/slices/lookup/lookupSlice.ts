import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICacheModel,
  IDataLocationModel,
  IIngestTypeModel,
  ILicenseModel,
  IMetricModel,
  IProductModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
} from 'tno-core';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  cache: [],
  actions: [],
  topics: [],
  rules: [],
  products: [],
  licenses: [],
  ingestTypes: [],
  roles: [],
  series: [],
  sources: [],
  sourceActions: [],
  metrics: [],
  tags: [],
  tonePools: [],
  users: [],
  dataLocations: [],
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
    storeTopics(state: ILookupState, action: PayloadAction<ITopicModel[]>) {
      state.topics = action.payload;
    },
    storeTopicScoreRules(state: ILookupState, action: PayloadAction<ITopicScoreRuleModel[]>) {
      state.rules = action.payload;
    },
    storeProducts(state: ILookupState, action: PayloadAction<IProductModel[]>) {
      state.products = action.payload;
    },
    storeSources(state: ILookupState, action: PayloadAction<ISourceModel[]>) {
      state.sources = action.payload;
    },
    storeLicenses(state: ILookupState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeIngestTypes(state: ILookupState, action: PayloadAction<IIngestTypeModel[]>) {
      state.ingestTypes = action.payload;
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
    storeMetrics(state: ILookupState, action: PayloadAction<IMetricModel[]>) {
      state.metrics = action.payload;
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
    storeDataLocations(state: ILookupState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
  },
});

export const {
  storeCache,
  updateCache,
  storeActions,
  storeTopics,
  storeTopicScoreRules,
  storeProducts,
  storeLicenses,
  storeIngestTypes,
  storeRoles,
  storeSeries,
  storeSources,
  storeSourceActions,
  storeMetrics,
  storeTags,
  storeTonePools,
  storeUsers,
  storeDataLocations,
} = lookupSlice.actions;
