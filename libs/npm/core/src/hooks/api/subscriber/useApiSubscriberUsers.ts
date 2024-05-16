import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { useApi } from '..';
import { ISubscriberUserModel } from '../interfaces/ISubscriberUserModel';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberUsers = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getUser: () => {
      return api.get<never, AxiosResponse<ISubscriberUserModel>, any>(`/subscriber/users`);
    },
    updateUser: (model: ISubscriberUserModel) => {
      return api.put<ISubscriberUserModel, AxiosResponse<ISubscriberUserModel>, any>(
        `/subscriber/users/${model.id}`,
        model,
      );
    },
  }).current;
};
