import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AdvancedSearchKeys } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { defaultPaperFilter } from 'features/content/papers/constants';
import { IPaperFilter } from 'features/content/papers/interfaces';
import { IContentModel, IPaged, LogicalOperator, saveToLocalStorage } from 'tno-core';

import { IContentState } from './interfaces';
import { castContentToSearchResult } from './utils';

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
    homepage: false,
    sort: [],
  },
  filterAdvanced: {
    fieldType: AdvancedSearchKeys.Source,
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
  filterPaper: defaultPaperFilter(),
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
    storeFilterPaper(state: IContentState, action: PayloadAction<IPaperFilter>) {
      state.filterPaper = action.payload;
    },
    storeFilterPaperAdvanced(
      state: IContentState,
      action: PayloadAction<IContentListAdvancedFilter>,
    ) {
      state.filterPaperAdvanced = action.payload;
    },
    storeContent(state: IContentState, action: PayloadAction<IPaged<IContentModel> | undefined>) {
      state.content = action.payload
        ? {
            ...action.payload,
            items: action.payload.items.map((content) => castContentToSearchResult(content)),
          }
        : action.payload;
      saveToLocalStorage('content', state.content?.items ?? []);
    },
    addContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = {
          ...state.content,
          items: [
            ...action.payload.map((c) => castContentToSearchResult(c)),
            ...state.content.items,
          ],
        };
      saveToLocalStorage('content', state.content?.items ?? []);
    },
    updateContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = {
          ...state.content,
          items: state.content.items.map((i) => {
            const content = action.payload.find((u) => u.id === i.id);
            return content ? castContentToSearchResult(content) : i;
          }),
        };
      saveToLocalStorage('content', state.content?.items ?? []);
    },
    removeContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.content)
        state.content = {
          ...state.content,
          items: state.content.items.filter((i) => !action.payload.some((r) => r.id === i.id)),
        };
      saveToLocalStorage('content', state.content?.items ?? []);
    },
  },
});

export const {
  storeFilter,
  storeFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
  addContent,
  storeContent,
  updateContent,
  removeContent,
} = contentSlice.actions;
