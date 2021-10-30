import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { MenuStatus } from 'tno-core';

import { INavState } from '.';

export const navSlice = createSlice({
  name: 'nav',
  initialState: {
    status: MenuStatus.hidden,
  },
  reducers: {
    storeStatus(state: INavState, action: PayloadAction<MenuStatus>) {
      state.status = action.payload;
    },
  },
});

export const { storeStatus } = navSlice.actions;
