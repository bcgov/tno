import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { type IWorkOrderFilter, WorkOrderTypeName } from 'tno-core';

import { type IWorkOrderState } from './interfaces';

export const initialWorkOrderState: IWorkOrderState = {
  transcriptFilter: {
    page: 1,
    quantity: 100,
    workType: WorkOrderTypeName.Transcription,
  },
};

export const workOrderSlice = createSlice({
  name: 'workOrder',
  initialState: initialWorkOrderState,
  reducers: {
    storeTranscriptFilter(state: IWorkOrderState, action: PayloadAction<IWorkOrderFilter>) {
      state.transcriptFilter = action.payload;
    },
  },
});

export const { storeTranscriptFilter } = workOrderSlice.actions;
