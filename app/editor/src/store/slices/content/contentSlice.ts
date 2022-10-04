import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fieldTypes } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentModel, IPaged, LogicalOperator } from 'hooks/api-editor';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  filter: {
    pageIndex: 0,
    pageSize: 100,
    includedInCategory: false,
    sourceId: 0,
    otherSource: '',
    productId: 0,
    ownerId: '',
    userId: '',
    timeFrame: 0,
    onTicker: '',
    commentary: '',
    topStory: '',
    sort: [],
  },
  filterAdvanced: {
    fieldType: fieldTypes[0].toInterface(),
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
};

export const contentSlice = createSlice({
  name: 'content',
  initialState: initialContentState,
  reducers: {
    storeFilter(state: IContentState, action: PayloadAction<IContentListFilter>) {
      state.filter = action.payload;
    },
    storeFilterAdvanced(state: IContentState, action: PayloadAction<IContentListAdvancedFilter>) {
      state.filterAdvanced = action.payload;
    },
    storeContent(state: IContentState, action: PayloadAction<IPaged<IContentModel>>) {
      state.content = action.payload;
    },
  },
});

export const { storeFilter, storeFilterAdvanced, storeContent } = contentSlice.actions;
