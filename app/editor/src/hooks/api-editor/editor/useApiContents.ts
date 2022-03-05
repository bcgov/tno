import React from 'react';
import { defaultEnvelope, extractResponseData, LifecycleToasts, toQueryString } from 'tno-core';

import { IContentFilter, IContentModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiContents = (
  options: {
    lifecycleToasts?: LifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useMemo(
    () => ({
      findContent: (filter?: IContentFilter) => {
        const params = {
          ...filter,
          actions: filter?.actions?.length ? filter.actions : undefined,
        };
        return extractResponseData<IPaged<IContentModel>>(() =>
          api.get(`/editor/contents?${toQueryString(params)}`),
        );
      },
      getContent: (id: number) => {
        return extractResponseData<IContentModel>(() => api.get(`/editor/contents/${id}`));
      },
      addContent: (content: IContentModel) => {
        return extractResponseData<IContentModel>(() => api.post('/editor/contents', content));
      },
      updateContent: (content: IContentModel) => {
        return extractResponseData<IContentModel>(() =>
          api.put(`/editor/contents/${content.id}`, content),
        );
      },
      deleteContent: (content: IContentModel) => {
        return extractResponseData<IContentModel>(() =>
          api.delete(`/editor/contents/${content.id}`, { data: content }),
        );
      },
    }),
    [api],
  );
};
