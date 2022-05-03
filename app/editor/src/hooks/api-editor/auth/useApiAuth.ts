import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IUserInfoModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAuth = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getUserInfo: () => {
      return api.post<IUserInfoModel>(`/auth/userinfo`);
    },
  }).current;
};
