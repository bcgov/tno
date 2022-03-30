import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IContentFilter, IContentModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiContents = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    findContent: (filter?: IContentFilter) => {
      const params = {
        ...filter,
        actions: filter?.actions?.length ? filter.actions : undefined,
      };
      return api.get<IPaged<IContentModel>>(`/editor/contents?${toQueryString(params)}`);
    },
    getContent: (id: number) => {
      return api.get<IContentModel>(`/editor/contents/${id}`);
    },
    addContent: (content: IContentModel) => {
      return api.post<IContentModel>('/editor/contents', content);
    },
    updateContent: (content: IContentModel) => {
      return api.put<IContentModel>(`/editor/contents/${content.id}`, content);
    },
    deleteContent: (content: IContentModel) => {
      return api.delete<IContentModel>(`/editor/contents/${content.id}`, { data: content });
    },
  };
};
