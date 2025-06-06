import {
  IndicesValidateQueryResponse,
  KnnSearchResponse,
  MsearchMultisearchBody,
} from '@elastic/elasticsearch/lib/api/types';
import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, IContentModel, ILifecycleToasts, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberContents = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findContentWithElasticsearch: (
      filter: MsearchMultisearchBody,
      includeUnpublishedContent: boolean = false,
    ) => {
      return api.post<MsearchMultisearchBody, AxiosResponse<KnnSearchResponse<IContentModel>>, any>(
        `/subscriber/contents/search${
          includeUnpublishedContent ? `?includeUnpublishedContent=${includeUnpublishedContent}` : ''
        }`,
        filter,
      );
    },
    validateElasticsearchQuery: (
      filter: MsearchMultisearchBody,
      includeUnpublishedContent: boolean = false,
      fieldNames: string = '',
    ) => {
      let params = includeUnpublishedContent
        ? `includeUnpublishedContent=${includeUnpublishedContent}`
        : '';
      const secondParam = fieldNames ? `fieldNames=${fieldNames}` : '';
      if (params && secondParam) {
        params = `${params}&&${secondParam}`;
      } else if (!params && secondParam) {
        params = secondParam;
      }
      if (params) {
        params = `?${params}`;
      }
      return api.post<MsearchMultisearchBody, AxiosResponse<IndicesValidateQueryResponse>, any>(
        `/subscriber/contents/validate${params}`,
        filter,
      );
    },
    getContent: (id: number) => {
      return api.get<never, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents/${id}`,
      );
    },
    download: async (id: number, fileName: string) => {
      const response = await api.get<never, AxiosResponse<any>, any>(
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
      const response = await api.get<never, AxiosResponse<any>, any>(
        `/subscriber/contents/stream?${toQueryString(params)}`,
        {
          responseType: 'blob',
          headers: { accept: '*.*' },
        },
      );

      response.data = window.URL.createObjectURL(new Blob([response.data]));
      return response;
    },
    addContent: (content: IContentModel) => {
      return api.post<IContentModel, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents`,
        content,
      );
    },
    updateContent: (content: IContentModel) => {
      return api.put<IContentModel, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents/${content.id}`,
        content,
      );
    },
    deleteContent: (content: IContentModel) => {
      return api.delete<IContentModel, AxiosResponse<IContentModel | undefined>, any>(
        `/subscriber/contents/${content.id}`,
        { data: content },
      );
    },
  }).current;
};
