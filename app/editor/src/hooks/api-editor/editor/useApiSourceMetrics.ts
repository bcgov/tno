import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ISourceMetricModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSourceMetrics = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getMetrics: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<ISourceMetricModel[]>(`/editor/source/metrics`, config);
    },
  };
};
