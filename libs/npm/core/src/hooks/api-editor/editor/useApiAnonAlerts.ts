import { AxiosResponse } from 'axios';
import React from 'react';

import { IAlertModel, ILifecycleToasts, defaultEnvelope, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAnonAlerts = (
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
      return api.get<IAlertModel, AxiosResponse<IAlertModel>, any>(`/editor/alerts`);
    },
  }).current;
};
