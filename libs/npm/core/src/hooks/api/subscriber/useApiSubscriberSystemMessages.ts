import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, ISystemMessageModel, useApi } from '../../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberSystemMessages = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findSystemMessages: () => {
      return api.get<never, AxiosResponse<ISystemMessageModel[]>, any>(
        `/subscriber/system-messages`,
      );
    },
  }).current;
};
