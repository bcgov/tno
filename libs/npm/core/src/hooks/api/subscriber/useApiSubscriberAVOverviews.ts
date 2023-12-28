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
    findAVOverview: (publishedOn?: Date | string | null) => {
      let url = '/subscriber/reports/av/overviews';
      if (publishedOn) {
        const params = {
          publishedOn,
        };
        url += `?${toQueryString(params)}`;
      }
      return api.get<never, AxiosResponse<IAVOverviewInstanceModel | undefined>, any>(url);
    },
    viewAVOverview: (instanceId: number) => {
      return api.post<never, AxiosResponse<never>, any>(
        `/subscriber/reports/av/overviews/${instanceId}/view`,
      );
    },
  }).current;
};
