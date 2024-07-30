import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  IMetricModel,
  IRoleModel,
  ISeriesModel,
  ISettingModel,
  ISourceActionModel,
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
} from 'tno-core';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  isReady: false,
  cache: [],
  actions: [],
  topics: [],
  mediaTypes: [],
  licenses: [],
  series: [],
  contributors: [],
  sources: [],
  sourceActions: [],
  tags: [],
  tonePools: [],
  holidays: [],
  ministers: [],
  ingestTypes: [],
  roles: [],
  metrics: [],
  users: [],
  dataLocations: [],
  systemMessages: [],
  settings: [],
  rules: [],
};

export const lookupSlice = createSlice({
  name: 'lookup',
  initialState: initialLookupState,
  reducers: {
    storeIsReady(state: ILookupState, action: PayloadAction<boolean>) {
      state.isReady = action.payload;
    },
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
    storeRules(state: ILookupState, action: PayloadAction<ITopicScoreRuleModel[]>) {
      state.rules = action.payload;
    },
    storeMinisters(state: ILookupState, action: PayloadAction<IMinisterModel[]>) {
      state.ministers = action.payload;
    },
    storeMediaTypes(state: ILookupState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeSources(state: ILookupState, action: PayloadAction<ISourceModel[]>) {
      state.sources = action.payload;
    },
    storeLicenses(state: ILookupState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeSeries(state: ILookupState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeContributors(state: ILookupState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeSourceActions(state: ILookupState, action: PayloadAction<ISourceActionModel[]>) {
      state.sourceActions = action.payload;
    },
    storeTags(state: ILookupState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeTonePools(state: ILookupState, action: PayloadAction<ITonePoolModel[]>) {
      state.tonePools = action.payload;
    },
    storeIngestTypes(state: ILookupState, action: PayloadAction<IIngestTypeModel[]>) {
      state.ingestTypes = action.payload;
    },
    storeRoles(state: ILookupState, action: PayloadAction<IRoleModel[]>) {
      state.roles = action.payload;
    },
    storeMetrics(state: ILookupState, action: PayloadAction<IMetricModel[]>) {
      state.metrics = action.payload;
    },
    storeUsers(state: ILookupState, action: PayloadAction<IUserModel[]>) {
      state.users = action.payload;
    },
    storeDataLocations(state: ILookupState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
    storeSystemMessages(state: ILookupState, action: PayloadAction<ISystemMessageModel[]>) {
      state.systemMessages = action.payload;
    },
    storeSettings(state: ILookupState, action: PayloadAction<ISettingModel[]>) {
      state.settings = action.payload;
    },
    storeSettingsFrontPageImagesMediaTypeId(state: ILookupState, action: PayloadAction<number>) {
      state.frontPageImagesMediaTypeId = action.payload;
    },
    storeHolidays(state: ILookupState, action: PayloadAction<IHolidayModel[]>) {
      state.holidays = action.payload;
    },
  },
});

export const {
  storeIsReady,
  storeCache,
  updateCache,
  storeActions,
  storeTopics,
  storeRules,
  storeMediaTypes,
  storeLicenses,
  storeSeries,
  storeContributors,
  storeSources,
  storeSourceActions,
  storeTags,
  storeTonePools,
  storeMinisters,
  storeHolidays,
  storeSettings,
  storeSystemMessages,
  storeUsers,
  storeMetrics,
  storeRoles,
  storeDataLocations,
  storeIngestTypes,
  storeSettingsFrontPageImagesMediaTypeId,
} = lookupSlice.actions;
