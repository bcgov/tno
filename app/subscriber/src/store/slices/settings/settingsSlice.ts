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
      state.commentaryActionId = action.payload.commentaryActionId;
      state.topStoryActionId = action.payload.topStoryActionId;
      state.featuredStoryActionId = action.payload.featuredStoryActionId;
      state.alertActionId = action.payload.alertActionId;
      state.editorUrl = action.payload.editorUrl;
      state.subscriberUrl = action.payload.subscriberUrl;
      state.defaultReportTemplateId = action.payload.defaultReportTemplateId;
    },
  },
});

export const { storeSettingsLoading, storeSettingsValues } = settingsSlice.actions;
