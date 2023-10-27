import { KnnSearchResponse, MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import {
  IContentFilter,
  IContentListModel,
  IContentModel,
  INotificationInstanceModel,
  IPaged,
  IWorkOrderModel,
  useApi,
} from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorContents = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findContent: (filter?: IContentFilter) => {
      const params = {
        ...filter,
        actions: filter?.actions?.length ? filter.actions : undefined,
      };
      return api.get<never, AxiosResponse<IPaged<IContentModel>>, any>(
        `/editor/contents?${toQueryString(params)}`,
      );
    },
    findContentWithElasticsearch: (
      filter: MsearchMultisearchBody,
      includeUnpublishedContent: boolean = false,
    ) => {
      return api.post<MsearchMultisearchBody, AxiosResponse<KnnSearchResponse<IContentModel>>, any>(
        `/editor/contents/search${
          includeUnpublishedContent ? `?includeUnpublishedContent=${includeUnpublishedContent}` : ''
        }`,
        filter,
      );
    },
    getContent: (id: number) => {
      return api.get<never, AxiosResponse<IContentModel | undefined>, any>(
        `/editor/contents/${id}`,
      );
    },
    addContent: (content: IContentModel) => {
      return api.post<IContentModel, AxiosResponse<IContentModel>, any>(
        '/editor/contents',
        content,
      );
    },
    updateContent: (content: IContentModel) => {
      return api.put<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${content.id}`,
        content,
      );
    },
    updateContentList: (action: IContentListModel) => {
      return api.put<IContentListModel, AxiosResponse<IContentModel[]>, any>(
        `/editor/contents`,
        action,
      );
    },
    deleteContent: (content: IContentModel) => {
      return api.delete<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${content.id}`,
        { data: content },
      );
    },
    transcribe: (content: IContentModel) => {
      return api.put<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/contents/${content.id}/transcribe`,
      );
    },
    nlp: (content: IContentModel) => {
      return api.put<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/contents/${content.id}/nlp`,
      );
    },
    publishContent: (content: IContentModel) => {
      return api.put<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${content.id}/publish`,
        content,
      );
    },
    unpublishContent: (content: IContentModel) => {
      return api.put<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${content.id}/unpublish`,
        content,
      );
    },
    upload: (
      content: IContentModel,
      file: File,
      onUploadProgress?: (progressEvent: any) => void,
    ) => {
      const formData = new FormData();
      formData.append('files', file, file.name);
      return api.post<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${content.id}/upload?version=${content.version}`,
        formData,
        { onUploadProgress },
      );
    },
    download: async (id: number, fileName: string) => {
      const response = await api.get<never, AxiosResponse<any>, any>(
        `/editor/contents/${id}/download`,
        {
          responseType: 'blob',
          headers: { accept: '*.*' },
        },
      );

      const uri = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = uri;
      link.setAttribute('download', fileName ?? new Date().toDateString());
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      return response;
    },
    attach: (contentId: number, locationId: number, path: string) => {
      const params = {
        path,
      };
      return api.put<IContentModel, AxiosResponse<IContentModel>, any>(
        `/editor/contents/${contentId}/${locationId}/attach?${toQueryString(params)}`,
      );
    },
    stream: async (path: string) => {
      const params = { path };
      const response = await api.get<never, AxiosResponse<any>, any>(
        `/editor/contents/stream?${toQueryString(params)}`,
        {
          responseType: 'blob',
          headers: { accept: '*.*' },
        },
      );

      response.data = window.URL.createObjectURL(new Blob([response.data]));
      return response;
    },
    getNotificationsFor: async (contentId: number) => {
      return api.get<never, AxiosResponse<INotificationInstanceModel[]>, any>(
        `/editor/contents/${contentId}/notifications`,
      );
    },
  }).current;
};
