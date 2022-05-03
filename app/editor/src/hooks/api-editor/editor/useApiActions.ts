import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IActionModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiActions = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getActions: (etag: string | undefined = undefined) => {
      const config = !!etag ? { headers: { 'If-None-Match': etag } } : undefined;
      return api.get<IActionModel[]>(`/editor/actions`, config);
    },
  };
};
