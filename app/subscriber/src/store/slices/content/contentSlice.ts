import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { IContentModel, IOptionItem, IPaged } from 'tno-core';

import { IContentState } from './interfaces';

export const initialContentState: IContentState = {
  searchFilter: {
    pageIndex: 0,
    pageSize: 100,
    otherSource: '',
    ownerId: '',
    userId: 0,
    inByline: true,
    inHeadline: true,
    inStory: true,
    mediaTypeIds: [],
    sourceIds: [],
    excludeSourceIds: [],
    contentTypes: [],
    sort: [],
  },
  homeFilter: {
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
    storeSearchFilter(state: IContentState, action: PayloadAction<IContentListFilter>) {
      state.searchFilter = action.payload;
    },
    storeHomeFilter(state: IContentState, action: PayloadAction<IContentListFilter>) {
      state.homeFilter = action.payload;
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
  storeSearchFilter,
  storeHomeFilter,
  addContent,
  storeContent,
  updateContent,
  removeContent,
  storeGalleryDateFilter,
  storeGalleryPressFilter,
} = contentSlice.actions;
