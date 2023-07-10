import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, ISystemMessageModel, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSystemMessages = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findSystemMessage: () => {
      return api.get<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/subscriber/system-message`,
      );
    },
  }).current;
};
