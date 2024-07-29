import { KnnSearchResponse, MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { ActionDelegate } from 'store';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';
import {
  IContentListModel,
  IContentModel,
  IContentTopicModel,
  INotificationInstanceModel,
  useApiEditorContents,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IContentController {
  findContentWithElasticsearch: (
    filter: MsearchMultisearchBody,
    includeUnpublishedContent: boolean,
  ) => Promise<KnnSearchResponse<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel | undefined>;
  addContent: (content: IContentModel) => Promise<IContentModel>;
  updateContent: (content: IContentModel) => Promise<IContentModel>;
  updateContentList: (content: IContentListModel) => Promise<IContentModel[]>;
  updateContentTopics: (id: number, topics?: IContentTopicModel[]) => Promise<IContentTopicModel[]>;
  deleteContent: (content: IContentModel) => Promise<IContentModel>;
  publishContent: (content: IContentModel) => Promise<IContentModel>;
  unpublishContent: (content: IContentModel) => Promise<IContentModel>;
  upload: (content: IContentModel, file: File) => Promise<IContentModel>;
  download: (id: number, fileName: string) => Promise<unknown>;
  attach: (contentId: number, locationId: number, path: string) => Promise<IContentModel>;
  storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => void;
  stream: (path: string) => Promise<string>;
  storeFilterAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
  storeFilterPaper: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => void;
  storeFilterPaperAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
  getNotificationsFor: (id: number) => Promise<INotificationInstanceModel[]>;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useAjaxWrapper();
  const api = useApiEditorContents();

  const controller = React.useMemo(() => {
    return {
      findContentWithElasticsearch: async (
        filter: MsearchMultisearchBody,
        includeUnpublishedContent: boolean,
      ) => {
        const response = await dispatch('find-contents-with-elasticsearch', () =>
          api.findContentWithElasticsearch(filter, includeUnpublishedContent),
        );
        return response.data;
      },
      getContent: async (id: number) => {
        const response = await dispatch('get-content', () => api.getContent(id), 'content');
        return response.status === 204 ? undefined : response.data;
      },
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
      updateContentList: async (content: IContentListModel) => {
        const response = await dispatch(
          'update-content-list',
          () => api.updateContentList(content),
          'content',
        );
        return response.data;
      },
      updateContentTopics: async (id: number, topics?: IContentTopicModel[]) => {
        const response = await dispatch(
          'update-content-topics',
          () => api.updateContentTopics(id, topics ?? []),
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
      publishContent: async (content: IContentModel) => {
        return (await dispatch('publish-content', () => api.publishContent(content), 'content'))
          .data;
      },
      unpublishContent: async (content: IContentModel) => {
        return (await dispatch('unpublish-content', () => api.unpublishContent(content), 'content'))
          .data;
      },
      upload: async (content: IContentModel, file: File) => {
        return (await dispatch('upload-content', () => api.upload(content, file), 'content')).data;
      },
      download: async (id: number, fileName: string) => {
        return (await dispatch('download-content', () => api.download(id, fileName), 'content'))
          .data;
      },
      attach: async (contentId: number, locationId: number, path: string) => {
        return (
          await dispatch<IContentModel>(
            'attach-content',
            () => api.attach(contentId, locationId, path),
            'content',
          )
        ).data;
      },
      stream: async (path: string) => {
        return (await dispatch('stream-content', () => api.stream(path), 'content')).data;
      },
      getNotificationsFor: async (id: number) => {
        return (
          await dispatch('content-notifications', () => api.getNotificationsFor(id), 'content')
        ).data;
      },
      storeFilter: actions.storeFilter,
      storeFilterAdvanced: actions.storeFilterAdvanced,
      storeFilterPaper: actions.storeFilterPaper,
      storeFilterPaperAdvanced: actions.storeFilterPaperAdvanced,
    };
  }, [actions, api, dispatch]);

  return [state, controller];
};
