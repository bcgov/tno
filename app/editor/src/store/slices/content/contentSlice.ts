import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fieldTypes } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportFilter } from 'features/content/morning-report/interfaces';
import { IContentModel, IPaged, LogicalOperator } from 'hooks/api-editor';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  filter: {
    pageIndex: 0,
    pageSize: 100,
    includedInTopic: false,
    includeHidden: false,
    sourceId: 0,
    otherSource: '',
    ownerId: '',
    userId: 0,
    productIds: [],
    sourceIds: [],
    timeFrame: 0,
    onTicker: '',
    commentary: '',
    topStory: '',
    sort: [],
  },
  filterAdvanced: {
    fieldType: fieldTypes[3].value,
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
  morningReportFilter: {
    pageIndex: 0,
    pageSize: 100,
    includedInTopic: false,
    includeHidden: false,
    sourceId: 0,
    otherSource: '',
    productIds: [],
    sourceIds: [],
    ownerId: '',
    userId: '',
    timeFrame: 0,
    onTicker: '',
    commentary: '',
    topStory: '',
    sort: [],
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
    storeMorningReportFilter(state: IContentState, action: PayloadAction<IMorningReportFilter>) {
      state.morningReportFilter = action.payload;
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
  storeMorningReportFilter,
  addContent,
  storeContent,
  updateContent,
  removeContent,
} = contentSlice.actions;
