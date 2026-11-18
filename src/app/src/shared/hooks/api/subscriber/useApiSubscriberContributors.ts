import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IContributorModel, ILifecycleToasts, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberContributors = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getContributors: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IContributorModel[], AxiosResponse<IContributorModel[]>, any>(
        `/subscriber/contributors`,
        config,
      );
    },
  }).current;
};
