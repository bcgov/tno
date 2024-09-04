import { KnnSearchResponse, MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import React from 'react';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';
import {
  IContentModel,
  IFilterSettingsModel,
  IOptionItem,
  useApiSubscriberContents,
} from 'tno-core';

import { useAjaxWrapper } from '..';

export interface IContentController {
  findContentWithElasticsearch: (
    filter: MsearchMultisearchBody,
    includeUnpublishedContent: boolean,
    store?: keyof IContentState,
  ) => Promise<KnnSearchResponse<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel | undefined>;
  download: (id: number, fileName: string) => Promise<unknown>;
  storeSearchFilter: (filter: IFilterSettingsModel) => void;
  storeHomeFilter: (filter: IFilterSettingsModel) => void;
  storeMyMinisterFilter: (filter: IFilterSettingsModel) => void;
  storeTopStoriesFilter: (filter: IFilterSettingsModel) => void;
  storeFrontPageFilter: (filter: IFilterSettingsModel) => void;
  storeTodaysCommentaryFilter: (filter: IFilterSettingsModel) => void;
  storeGalleryDateFilter: (dateFilter: IOptionItem | null) => void;
  storeGalleryPressFilter: (pressFilter: IOptionItem | null) => void;
  storeAvOverviewDateFilter: (filter: IFilterSettingsModel) => void;
  storeEventofTheDayDateFilter: (filter: IFilterSettingsModel) => void;
  storeMediaTypeFilter: (filter: IFilterSettingsModel) => void;
  storeSearchResultsFilter: (filter: IFilterSettingsModel) => void;
  stream: (path: string) => Promise<string>;
  addContent: (content: IContentModel) => Promise<IContentModel | undefined>;
  updateContent: (content: IContentModel) => Promise<IContentModel | undefined>;
  updateContentSilent: (content: IContentModel) => Promise<IContentModel | undefined>;
  deleteContent: (content: IContentModel) => Promise<IContentModel | undefined>;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useAjaxWrapper();
  const api = useApiSubscriberContents();

  const controller = React.useMemo(
    () => ({
      findContentWithElasticsearch: async (
        filter: MsearchMultisearchBody,
        includeUnpublishedContent: boolean,
        store?: keyof IContentState,
      ) => {
        const response = await dispatch('find-contents-with-elasticsearch', () =>
          api.findContentWithElasticsearch(filter, includeUnpublishedContent),
        );
        if (!!store) {
          switch (store) {
            case 'frontPage':
              actions.storeFrontPageContent(response.data);
              break;
            case 'home':
              actions.storeHomeContent(response.data);
              break;
            case 'mediaType':
              actions.storeMediaTypeContent(response.data);
              break;
            case 'myMinister':
              actions.storeMyMinisterContent(response.data);
              break;
            case 'search':
              actions.storeSearchContent(response.data);
              break;
            case 'todaysCommentary':
              actions.storeTodaysCommentaryContent(response.data);
              break;
            case 'topStories':
              actions.storeTopStoriesContent(response.data);
              break;
          }
        }
        return response.data;
      },
      getContent: async (id: number) => {
        const response = await dispatch('get-content', () => api.getContent(id), 'content');
        return response.status === 204 ? undefined : response.data;
      },
      download: async (id: number, fileName: string) => {
        return (await dispatch('download-content', () => api.download(id, fileName), 'content'))
          .data;
      },
      stream: async (path: string) => {
        return (await dispatch('stream-content', () => api.stream(path), 'content')).data;
      },
      storeSearchFilter: actions.storeSearchFilter,
      storeHomeFilter: actions.storeHomeFilter,
      storeTopStoriesFilter: actions.storeTopStoriesFilter,
      storeTodaysCommentaryFilter: actions.storeTodaysCommentaryFilter,
      storeGalleryDateFilter: actions.storeGalleryDateFilter,
      storeGalleryPressFilter: actions.storeGalleryPressFilter,
      storeAvOverviewDateFilter: actions.storeAvOverviewDateFilter,
      storeEventofTheDayDateFilter: actions.storeEventofTheDayDateFilter,
      storeFrontPageFilter: actions.storeFrontPageFilter,
      storeMediaTypeFilter: actions.storeMediaTypeFilter,
      storeMyMinisterFilter: actions.storeMyMinisterFilter,
      storeSearchResultsFilter: actions.storeSearchResultsFilter,
      addContent: async (content: IContentModel) => {
        const response = await dispatch('add-content', () => api.addContent(content), 'content');
        return response.data;
      },
      updateContent: async (content: IContentModel) => {
        const response = await dispatch(
          'update-content',
          () => api.updateContent(content),
          'content',
        );
        return response.data;
      },
      updateContentSilent: async (content: IContentModel) => {
        const response = await dispatch(
          'update-content',
          () => api.updateContent(content),
          'content',
          true,
          true,
        );
        return response.data;
      },
      deleteContent: async (content: IContentModel) => {
        const response = await dispatch(
          'delete-content',
          () => api.deleteContent(content),
          'content',
        );
        return response.data;
      },
    }),
    [actions, api, dispatch],
  );

  return [state, controller];
};
