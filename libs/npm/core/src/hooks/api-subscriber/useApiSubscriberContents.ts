import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../utils';
import {
  defaultEnvelope,
  IContentFilter,
  IContentModel,
  ILifecycleToasts,
  IPaged,
  useApi,
} from '..';

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
    getFrontPages: () => {
      return api.get<IPaged<IContentModel>, AxiosResponse<IPaged<IContentModel>>, any>(
        `/subscriber/contents/frontpages`,
      );
    },
    getContent: (id: number) => {
      return api.get<IContentModel | undefined, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents/${id}`,
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
      const response = await api.get<any, AxiosResponse<any>, any>(
        `/subscriber/contents/stream?${toQueryString(params)}`,
        {
          responseType: 'blob',
          headers: { accept: '*.*' },
        },
      );

      response.data = window.URL.createObjectURL(new Blob([response.data]));
      return response;
    },
  }).current;
};
