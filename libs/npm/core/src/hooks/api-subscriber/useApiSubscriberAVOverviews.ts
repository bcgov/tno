import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IAVOverviewInstanceModel, ILifecycleToasts, useApi } from '../../hooks';
import { toQueryString } from '../../utils';

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
        `/editor/reports/av/overviews?${toQueryString(params)}`,
      );
    },
    viewAVOverview: (instanceId: number) => {
      return api.post<never, AxiosResponse<never>, any>(
        `/subscriber/reports/av/overviews/${instanceId}/view`,
      );
    },
  }).current;
};
