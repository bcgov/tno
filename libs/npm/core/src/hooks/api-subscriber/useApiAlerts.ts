import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IAlertModel, ILifecycleToasts, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAlerts = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAlert: () => {
      return api.get<IAlertModel, AxiosResponse<IAlertModel>, any>(`/subscriber/alerts`);
    },
  }).current;
};
