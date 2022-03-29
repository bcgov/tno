import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { ISourceMetricModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
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
    getMetrics: () => {
      return extractResponseData<ISourceMetricModel[]>(() => api.get(`/editor/source/metrics`));
    },
  };
};
