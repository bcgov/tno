import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AdvancedSearchKeys } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import { LogicalOperator } from 'tno-core';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  filter: {
    pageIndex: 0,
    pageSize: 500,
    hasTopic: false,
    isHidden: false,
    onlyPublished: false,
    otherSource: '',
    ownerId: '',
    userId: 0,
    contentTypes: [],
    mediaTypeIds: [],
    sourceIds: [],
    excludeSourceIds: [],
    timeFrame: 0,
    commentary: false,
    topStory: false,
    featuredStory: false,
    sort: [],
  },
  filterAdvanced: {
    fieldType: AdvancedSearchKeys.Source,
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
  filterPaper: {
    pageIndex: 0,
    pageSize: 500,
    hasTopic: false,
    isHidden: false,
    onlyPublished: false,
    otherSource: '',
    ownerId: '',
    userId: 0,
    contentTypes: [],
    mediaTypeIds: [],
    sourceIds: [],
    excludeSourceIds: [],
    timeFrame: 0,
    commentary: false,
    topStory: false,
    featuredStory: false,
    sort: [{ id: 'source.sortOrder' }, { id: 'otherSource' }, { id: 'page' }],
  },
  filterPaperAdvanced: {
    fieldType: AdvancedSearchKeys.Headline,
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
    storeFilterPaper(state: IContentState, action: PayloadAction<IContentListFilter>) {
      state.filterPaper = action.payload;
    },
    storeFilterPaperAdvanced(
      state: IContentState,
      action: PayloadAction<IContentListAdvancedFilter>,
    ) {
      state.filterPaperAdvanced = action.payload;
    },
  },
});

export const {
  storeFilter: storeContentFilter,
  storeFilterAdvanced: storeContentFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
} = contentSlice.actions;
