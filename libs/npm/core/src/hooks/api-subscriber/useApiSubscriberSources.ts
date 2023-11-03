import { AxiosResponse } from 'axios';
import React from 'react';

import { ILifecycleToasts, IPaged, ISourceModel, defaultEnvelope, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberSources = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllSources: () => {
      return api.get<never, AxiosResponse<ISourceModel[]>, any>(`/subscriber/sources`);
    },
  }).current;
};
