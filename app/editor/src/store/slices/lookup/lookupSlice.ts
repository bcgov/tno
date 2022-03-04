import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  IMediaTypeModel,
  ISeriesModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks';

import { ILookupState } from './interfaces';

export const initialLookupState: ILookupState = {
  actions: [],
  categories: [],
  contentTypes: [],
  mediaTypes: [],
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
    storeCategories(state: ILookupState, action: PayloadAction<ICategoryModel[]>) {
      state.categories = action.payload;
    },
    storeContentTypes(state: ILookupState, action: PayloadAction<IContentTypeModel[]>) {
      state.contentTypes = action.payload;
    },
    storeMediaTypes(state: ILookupState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
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
  storeCategories,
  storeContentTypes,
  storeMediaTypes,
  storeSeries,
  storeTags,
  storeTonePools,
  storeUsers,
} = lookupSlice.actions;
