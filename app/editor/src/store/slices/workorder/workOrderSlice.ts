import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IPaged } from 'hooks';

import { IWorkOrderModel, IWorkOrderState } from './interfaces';
import { IWorkOrderListFilter } from './interfaces/IWorkOrderListFilter';

export const initialWorkOrderState: IWorkOrderState = {
  workOrderFilter: {
    pageIndex: 0,
    pageSize: 10,
    sort: [],
  },
  workOrders: { page: 1, quantity: 10, items: [], total: 0 },
};

export const workOrderSlice = createSlice({
  name: 'workOrders',
  initialState: initialWorkOrderState,
  reducers: {
    storeWorkOrderFilter(state: IWorkOrderState, action: PayloadAction<IWorkOrderListFilter>) {
      state.workOrderFilter = action.payload;
    },
    storeWorkOrders(state: IWorkOrderState, action: PayloadAction<IPaged<IWorkOrderModel>>) {
      state.workOrders = action.payload;
    },
  },
});

export const { storeWorkOrderFilter, storeWorkOrders } = workOrderSlice.actions;
