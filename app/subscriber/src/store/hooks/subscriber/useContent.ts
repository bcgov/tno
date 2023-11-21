import { KnnSearchResponse, MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import React from 'react';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';
import {
  IContentFilter,
  IContentModel,
  IOptionItem,
  IPaged,
  useApiSubscriberContents,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IContentController {
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  findContentWithElasticsearch: (
    filter: MsearchMultisearchBody,
    includeUnpublishedContent: boolean,
  ) => Promise<KnnSearchResponse<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel | undefined>;
  getFrontPages: () => Promise<IPaged<IContentModel>>;
  download: (id: number, fileName: string) => Promise<unknown>;
  storeSearchFilter: (filter: IContentListFilter) => void;
  storeHomeFilter: (filter: IContentListFilter) => void;
  storeGalleryDateFilter: (dateFilter: IOptionItem | null) => void;
  storeGalleryPressFilter: (pressFilter: IOptionItem | null) => void;
  stream: (path: string) => Promise<string>;
  addContent: (content: IContentModel) => Promise<IContentModel | undefined>;
  updateContent: (content: IContentModel) => Promise<IContentModel | undefined>;
  deleteContent: (content: IContentModel) => Promise<IContentModel | undefined>;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useAjaxWrapper();
  const api = useApiSubscriberContents();

  const controller = React.useMemo(
    () => ({
      findContent: async (filter: IContentFilter) => {
        const response = await dispatch('find-contents', () => api.findContent(filter));
        actions.storeContent(response.data);
        return response.data;
      },
      findContentWithElasticsearch: async (
        filter: MsearchMultisearchBody,
        includeUnpublishedContent: boolean,
      ) => {
        const response = await dispatch('find-contents-with-elasticsearch', () =>
          api.findContentWithElasticsearch(filter, includeUnpublishedContent),
        );
        return response.data;
      },
      getFrontPages: async () => {
        const response = await dispatch('find-contents', () => api.getFrontPages());
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
      storeGalleryDateFilter: actions.storeGalleryDateFilter,
      storeGalleryPressFilter: actions.storeGalleryPressFilter,
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
