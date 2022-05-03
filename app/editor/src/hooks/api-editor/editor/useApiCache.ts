import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ICacheModel, useApi } from '..';

/**
 * Common hook to make requests to the APi.
 * @returns CustomAxios object setup for the API.
 */
export const useApiCache = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getCache: () => {
      return api.get<ICacheModel[]>(`/editor/cache`);
    },
  };
};
