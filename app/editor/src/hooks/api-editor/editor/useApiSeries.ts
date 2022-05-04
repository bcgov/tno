import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ISeriesModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
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
    getSeries: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<ISeriesModel[]>(`/editor/series`, config);
    },
  };
};
