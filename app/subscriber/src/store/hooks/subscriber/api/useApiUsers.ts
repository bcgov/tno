import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, IUserModel, useApi } from 'tno-core';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiUsers = (
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
