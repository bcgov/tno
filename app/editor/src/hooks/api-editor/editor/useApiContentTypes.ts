import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IContentTypeModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiContentTypes = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getContentTypes: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IContentTypeModel[]>(`/editor/content/types`, config);
    },
  };
};
