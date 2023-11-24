import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, IAVOverviewInstanceModel, ILifecycleToasts, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberAVOverviews = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAVOverview: (publishedOn: Date | string) => {
      const params = {
        publishedOn,
      };
      return api.get<never, AxiosResponse<IAVOverviewInstanceModel | undefined>, any>(
        `/subscriber/reports/av/overviews?${toQueryString(params)}`,
      );
    },
    viewAVOverview: (instanceId: number) => {
      return api.post<never, AxiosResponse<never>, any>(
        `/subscriber/reports/av/overviews/${instanceId}/view`,
      );
    },
  }).current;
};
