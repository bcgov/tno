import { AxiosResponse } from 'axios';
import React from 'react';

import { IUserModel, useApi } from '../../hooks/api-editor';
import { defaultEnvelope, ILifecycleToasts } from '../../hooks/summon';

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
    updateUser: (model: IUserModel) => {
      return api.put<IUserModel, AxiosResponse<IUserModel>, any>(
        `/subscriber/users/${model.id}`,
        model,
      );
    },
  }).current;
};
