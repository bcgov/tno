import React from 'react';
import { defaultEnvelope, extractResponseData, LifecycleToasts } from 'tno-core';

import { ISeriesModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiSeries = (
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
      getSeries: () => {
        return extractResponseData<ISeriesModel[]>(() => api.get(`/editor/series`));
      },
    }),
    [api],
  );
};
