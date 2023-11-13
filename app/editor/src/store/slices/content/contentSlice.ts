import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AdvancedSearchKeys } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import { IContentModel, IPaged, LogicalOperator, saveToLocalStorage } from 'tno-core';

import { IContentState } from './interfaces';
import { castContentToSearchResult } from './utils';

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
    storeContent(state: IContentState, action: PayloadAction<IPaged<IContentModel> | undefined>) {
      state.searchResults = action.payload
        ? {
            ...action.payload,
            items: action.payload.items.map((content) => castContentToSearchResult(content)),
          }
        : action.payload;
      saveToLocalStorage('content', state.searchResults?.items ?? []);
    },
    addContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.searchResults)
        state.searchResults = {
          ...state.searchResults,
          items: [
            ...action.payload.map((c) => castContentToSearchResult(c)),
            ...state.searchResults.items,
          ],
        };
      saveToLocalStorage('content', state.searchResults?.items ?? []);
    },
    updateContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.searchResults)
        state.searchResults = {
          ...state.searchResults,
          items: state.searchResults.items.map((i) => {
            const content = action.payload.find((u) => u.id === i.id);
            return content ? castContentToSearchResult(content) : i;
          }),
        };
      saveToLocalStorage('content', state.searchResults?.items ?? []);
    },
    removeContent(state: IContentState, action: PayloadAction<IContentModel[]>) {
      if (!!state.searchResults)
        state.searchResults = {
          ...state.searchResults,
          items: state.searchResults.items.filter(
            (i) => !action.payload.some((r) => r.id === i.id),
          ),
        };
      saveToLocalStorage('content', state.searchResults?.items ?? []);
    },
  },
});

export const {
  storeFilter: storeContentFilter,
  storeFilterAdvanced: storeContentFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
  addContent,
  storeContent,
  updateContent,
  removeContent,
} = contentSlice.actions;
