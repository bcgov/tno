import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { ISeriesModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiSeries = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getSeries: () => {
      return extractResponseData<ISeriesModel[]>(() => api.get(`/editor/series`));
    },
  };
};
