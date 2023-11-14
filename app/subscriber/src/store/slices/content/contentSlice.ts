import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fieldTypes } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentModel, IOptionItem, IPaged, LogicalOperator } from 'tno-core';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  filter: {
    pageIndex: 0,
    pageSize: 100,
    otherSource: '',
    ownerId: '',
    userId: 0,
    mediaTypeIds: [],
    sourceIds: [],
    excludeSourceIds: [],
    contentTypes: [],
    sort: [],
  },
  filterAdvanced: {
    fieldType: fieldTypes[3].value,
    logicalOperator: LogicalOperator.Contains,
    searchTerm: '',
  },
  pressGalleryFilter: {
    dateFilter: null,
    pressFilter: null,
  },
};

export const contentSlice = createSlice({
  name: 'content',
  initialState: initialContentState,
  reducers: {
    storeGalleryPressFilter(state: IContentState, action: PayloadAction<IOptionItem | null>) {
      state.pressGalleryFilter.pressFilter = action.payload;
    },
    storeGalleryDateFilter(state: IContentState, action: PayloadAction<IOptionItem | null>) {
      state.pressGalleryFilter.dateFilter = action.payload;
    },
    storeFilter(state: IContentState, action: PayloadAction<IContentListFilter>) {
      state.filter = action.payload;
    },
    storeFilterAdvanced(state: IContentState, action: PayloadAction<IContentListAdvancedFilter>) {
      state.filterAdvanced = action.payload;
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
  addContent,
  storeContent,
  updateContent,
  removeContent,
  storeGalleryDateFilter,
  storeGalleryPressFilter,
} = contentSlice.actions;
