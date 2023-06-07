import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { advancedSearchKeys } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { defaultMorningReportsFilter } from 'features/content/morning-papers/constants';
import { IMorningReportsFilter } from 'features/content/morning-papers/interfaces';
import { IContentModel, IPaged, LogicalOperator } from 'tno-core';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  filter: {
    pageIndex: 0,
    pageSize: 500,
    hasTopic: false,
    includeHidden: false,
    onlyHidden: false,
    onlyPublished: false,
    otherSource: '',
    ownerId: '',
    userId: 0,
    contentTypes: [],
    productIds: [],
    sourceIds: [],
    excludeSourceIds: [],
    timeFrame: 0,
    onTicker: false,
    commentary: false,
    topStory: false,
    sort: [],
  },
  filterAdvanced: {
    fieldType: advancedSearchKeys.Source,
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
  filterMorningReports: defaultMorningReportsFilter(),
  filterMorningReportAdvanced: {
    fieldType: advancedSearchKeys.Headline,
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
    storeFilterMorningReport(state: IContentState, action: PayloadAction<IMorningReportsFilter>) {
      state.filterMorningReports = action.payload;
    },
    storeFilterMorningReportAdvanced(
      state: IContentState,
      action: PayloadAction<IContentListAdvancedFilter>,
    ) {
      state.filterMorningReportAdvanced = action.payload;
    },
    storeContent(state: IContentState, action: PayloadAction<IPaged<IContentModel> | undefined>) {
      state.content = action.payload;
    },
    addContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = { ...state.content, items: [...action.payload, ...state.content.items] };
    },
    updateContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = {
          ...state.content,
          items: state.content.items.map((i) => action.payload.find((u) => u.id === i.id) ?? i),
        };
    },
    removeContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = {
          ...state.content,
          items: state.content.items.filter((i) => !action.payload.some((r) => r.id === i.id)),
        };
    },
  },
});

export const {
  storeFilter,
  storeFilterAdvanced,
  storeFilterMorningReport,
  storeFilterMorningReportAdvanced,
  addContent,
  storeContent,
  updateContent,
  removeContent,
} = contentSlice.actions;
