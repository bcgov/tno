import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ICacheModel, ILifecycleToasts, useApi } from '..';

/**
 * Common hook to make requests to the APi.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberCache = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getCache: () => {
      return api.get<ICacheModel[], AxiosResponse<ICacheModel[]>, any>(`/subscriber/cache`);
    },
  }).current;
};
