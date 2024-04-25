import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IReportResultForm } from 'features/my-reports/interfaces';

import { IReportsState } from './interfaces';

export const initialReportsState: IReportsState = {};

export const reportsSlice = createSlice({
  name: 'reports',
  initialState: initialReportsState,
  reducers: {
    storeReportView(state: IReportsState, action: PayloadAction<IReportResultForm | undefined>) {
      state.reportView = action.payload;
    },
  },
});

export const { storeReportView } = reportsSlice.actions;
