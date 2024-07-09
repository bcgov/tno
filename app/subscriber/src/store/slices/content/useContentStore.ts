import { KnnSearchResponse } from '@elastic/elasticsearch/lib/api/types';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { IContentModel, IFilterSettingsModel, IOptionItem } from 'tno-core';

import {
  storeAvOverviewDateFilter,
  storeEventofTheDayDateFilter,
  storeFrontPageContent,
  storeFrontPageFilter,
  storeGalleryDateFilter,
  storeGalleryPressFilter,
  storeHomeContent,
  storeHomeFilter,
  storeMediaTypeContent,
  storeMediaTypeFilter,
  storeMyMinisterContent,
  storeMyMinisterFilter,
  storeSearchContent,
  storeSearchFilter,
  storeTodaysCommentaryContent,
  storeTodaysCommentaryFilter,
  storeTopStoriesContent,
  storeTopStoriesFilter,
} from '.';
import { IContentState } from './interfaces';

export interface IContentProps {
  filter: IFilterSettingsModel;
}

export interface IContentStore {
  storeGalleryDateFilter: (date: IOptionItem | null) => void;
  storeGalleryPressFilter: (filter: IOptionItem | null) => void;
  storeAvOverviewDateFilter: (filter: IFilterSettingsModel) => void;
  storeEventofTheDayDateFilter: (filter: IFilterSettingsModel) => void;
  storeFrontPageFilter: (filter: IFilterSettingsModel) => void;
  storeMediaTypeFilter: (filter: IFilterSettingsModel) => void;
  storeSearchFilter: (filter: IFilterSettingsModel) => void;
  storeMyMinisterFilter: (filter: IFilterSettingsModel) => void;
  storeTodaysCommentaryFilter: (filter: IFilterSettingsModel) => void;
  storeHomeFilter: (filter: IFilterSettingsModel) => void;
  storeTopStoriesFilter: (filter: IFilterSettingsModel) => void;
  storeSearchContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeFrontPageContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeHomeContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeMediaTypeContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeMyMinisterContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeTodaysCommentaryContent: (content: KnnSearchResponse<IContentModel>) => void;
  storeTopStoriesContent: (content: KnnSearchResponse<IContentModel>) => void;
}

export const useContentStore = (props?: IContentProps): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);

  const controller = React.useMemo(
    () => ({
      storeGalleryPressFilter: (filter: IOptionItem | null) => {
        dispatch(storeGalleryPressFilter(filter));
      },
      storeGalleryDateFilter: (date: IOptionItem | null) => {
        dispatch(storeGalleryDateFilter(date));
      },
      storeAvOverviewDateFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeAvOverviewDateFilter(filter));
      },
      storeEventofTheDayDateFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeEventofTheDayDateFilter(filter));
      },
      storeSearchFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeSearchFilter(filter));
      },
      storeTodaysCommentaryFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeTodaysCommentaryFilter(filter));
      },
      storeHomeFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeHomeFilter(filter));
      },
      storeMyMinisterFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeMyMinisterFilter(filter));
      },
      storeFrontPageFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeFrontPageFilter(filter));
      },
      storeTopStoriesFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeTopStoriesFilter(filter));
      },
      storeMediaTypeFilter: (filter: IFilterSettingsModel) => {
        dispatch(storeMediaTypeFilter(filter));
      },
      storeSearchContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeSearchContent(content));
      },
      storeFrontPageContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeFrontPageContent(content));
      },
      storeHomeContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeHomeContent(content));
      },
      storeMediaTypeContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeMediaTypeContent(content));
      },
      storeMyMinisterContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeMyMinisterContent(content));
      },
      storeTodaysCommentaryContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeTodaysCommentaryContent(content));
      },
      storeTopStoriesContent: (content: KnnSearchResponse<IContentModel>) => {
        dispatch(storeTopStoriesContent(content));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
