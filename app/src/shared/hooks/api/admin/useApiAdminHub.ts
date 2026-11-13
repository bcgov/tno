import { AxiosResponse } from 'axios';
import React from 'react';

import { HubEventsName, MessageTargetName, useApi } from '..';
import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminHub = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    sendMessage: (hubEvent: HubEventsName, target: MessageTargetName, message: any) => {
      const query = toQueryString({ hubEvent, target });
      return api.post<any, AxiosResponse, any>(`/admin/hub?${query}`, message);
    },
  }).current;
};
