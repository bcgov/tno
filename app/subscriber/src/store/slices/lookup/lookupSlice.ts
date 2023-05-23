import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IHolidayModel,
  ILicenseModel,
  IProductModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
} from 'tno-core';

import { ILookupState } from './interfaces';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';

export const initialLookupState: ILookupState = {
  cache: [],
  actions: [],
  topics: [],
  products: [],
  licenses: [],
  series: [],
  contributors: [],
  sources: [],
  sourceActions: [],
  tags: [],
  tonePools: [],
  holidays: [],
  ministers: [],
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
    storeMinisters(state: ILookupState, action: PayloadAction<IMinisterModel[]>) {
      state.ministers = action.payload;
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
    storeHolidays(state: ILookupState, action: PayloadAction<IHolidayModel[]>) {
      state.holidays = action.payload;
    },
  },
});

export const {
  storeCache,
  updateCache,
  storeActions,
  storeTopics,
  storeProducts,
  storeLicenses,
  storeSeries,
  storeContributors,
  storeSources,
  storeSourceActions,
  storeTags,
  storeTonePools,
  storeMinisters,
  storeHolidays,
} = lookupSlice.actions;
