import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { ISettingsState } from './interfaces';

export const initialSettingsState: ISettingsState = {
  loadingState: 0,
  isReady: false,
};

export const settingsSlice = createSlice({
  name: 'settings',
  initialState: initialSettingsState,
  reducers: {
    storeSettingsLoading(state: ISettingsState, action: PayloadAction<number>) {
      state.loadingState = action.payload;
    },
    storeSettingsValues(state: ISettingsState, action: PayloadAction<ISettingsState>) {
      state.isReady = action.payload.isReady;
      state.loadingState = action.payload.loadingState;
      state.featuredStoryActionId = action.payload.featuredStoryActionId;
      state.commentaryActionId = action.payload.commentaryActionId;
      state.disableTranscriptionMediaTypeIds = action.payload.disableTranscriptionMediaTypeIds;
      state.topStoryActionId = action.payload.topStoryActionId;
      state.alertActionId = action.payload.alertActionId;
      state.editorUrl = action.payload.editorUrl;
      state.subscriberUrl = action.payload.subscriberUrl;
      state.defaultReportTemplateId = action.payload.defaultReportTemplateId;
      state.frontpageFilterId = action.payload.frontpageFilterId;
      state.frontPageImageMediaTypeId = action.payload.frontPageImageMediaTypeId;
      state.excludeBylineIds = action.payload.excludeBylineIds;
      state.excludeSourceIds = action.payload.excludeSourceIds;
      state.eventOfTheDayReportId = action.payload.eventOfTheDayReportId;
    },
  },
});

export const { storeSettingsLoading, storeSettingsValues } = settingsSlice.actions;
