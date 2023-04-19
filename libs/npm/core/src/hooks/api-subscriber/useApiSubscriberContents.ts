import { AxiosResponse } from 'axios';
import React from 'react';

import {
  IContentFilter,
  IContentListModel,
  IContentModel,
  ILifecycleToasts,
  IPaged,
  defaultEnvelope,
  useApi,
} from '..';
import { toQueryString } from '../../utils';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useSubscriberApiContents = (
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
      return api.get<IPaged<IContentModel>, AxiosResponse<IPaged<IContentModel>>, any>(
        `/subscriber/contents?${toQueryString(params)}`,
      );
    },
    getContent: (id: number) => {
      return api.get<IContentModel | undefined, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents/${id}`,
      );
    },
    updateContent: (content: IContentModel) => {
      return api.put<IContentModel, AxiosResponse<IContentModel>, any>(
        `/subscriber/contents/${content.id}`,
        content,
      );
    },
    updateContentList: (action: IContentListModel) => {
      return api.put<IContentListModel, AxiosResponse<IContentModel[]>, any>(
        `/subscriber/contents`,
        action,
      );
    },
    download: async (id: number, fileName: string) => {
      const response = await api.get<any, AxiosResponse<any>, any>(
        `/subscriber/contents/${id}/download`,
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
    stream: async (path: string) => {
      const params = { path };
      return await api.get<any, AxiosResponse<any>, any>(
        `/subscriber/contents/stream?${toQueryString(params)}`,
      );
    },
  }).current;
};
