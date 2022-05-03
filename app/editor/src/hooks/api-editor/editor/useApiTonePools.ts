import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ITonePoolModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiTonePools = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getTonePools: (etag: string | undefined = undefined) => {
      const config = !!etag ? { headers: { 'If-None-Match': etag } } : undefined;
      return api.get<ITonePoolModel[]>(`/editor/tone/pools`, config);
    },
  };
};
