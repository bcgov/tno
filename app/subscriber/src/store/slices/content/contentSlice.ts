import { KnnSearchResponse } from '@elastic/elasticsearch/lib/api/types';
import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IContentModel, IFilterSettingsModel, IOptionItem } from 'tno-core';

import { initialContentState } from './constants';
import { IContentState } from './interfaces';

export const contentSlice = createSlice({
  name: 'content',
  initialState: initialContentState,
  reducers: {
    storeGalleryPressFilter(state: IContentState, action: PayloadAction<IOptionItem | null>) {
      state.pressGalleryFilter.pressFilter = action.payload;
    },
    storeGalleryDateFilter(state: IContentState, action: PayloadAction<IOptionItem | null>) {
      state.pressGalleryFilter.dateFilter = action.payload;
    },
    storeAvOverviewDateFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.avOverview.filter = action.payload;
    },
    storeEventofTheDayDateFilter(
      state: IContentState,
      action: PayloadAction<IFilterSettingsModel>,
    ) {
      state.eventOfTheDay.filter = action.payload;
    },
    storeSearchFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.search.filter = action.payload;
    },
    storeHomeFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.home.filter = action.payload;
    },
    storeFrontPageFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.frontPage.filter = action.payload;
    },
    storeMediaTypeFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.mediaType.filter = action.payload;
    },
    storeMyMinisterFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.myMinister.filter = action.payload;
    },
    storeTodaysCommentaryFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.todaysCommentary.filter = action.payload;
    },
    storeTopStoriesFilter(state: IContentState, action: PayloadAction<IFilterSettingsModel>) {
      state.topStories.filter = action.payload;
    },
    storeSearchContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.search.content = action.payload;
    },
    storeFrontPageContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.frontPage.content = action.payload;
    },
    storeHomeContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.home.content = action.payload;
    },
    storeMediaTypeContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.mediaType.content = action.payload;
    },
    storeMyMinisterContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.myMinister.content = action.payload;
    },
    storeTodaysCommentaryContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.todaysCommentary.content = action.payload;
    },
    storeTopStoriesContent(
      state: IContentState,
      action: PayloadAction<KnnSearchResponse<IContentModel> | undefined>,
    ) {
      state.topStories.content = action.payload;
    },
  },
});

export const {
  storeSearchFilter,
  storeHomeFilter,
  storeMyMinisterFilter,
  storeFrontPageFilter,
  storeTopStoriesFilter,
  storeTodaysCommentaryFilter,
  storeSearchContent,
  storeFrontPageContent,
  storeGalleryDateFilter,
  storeGalleryPressFilter,
  storeAvOverviewDateFilter,
  storeEventofTheDayDateFilter,
  storeHomeContent,
  storeMediaTypeContent,
  storeMyMinisterContent,
  storeTodaysCommentaryContent,
  storeTopStoriesContent,
  storeMediaTypeFilter,
} = contentSlice.actions;
