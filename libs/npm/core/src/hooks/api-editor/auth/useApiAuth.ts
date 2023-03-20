import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IRegisterModel, IUserInfoModel, IUserModel, useApi } from '..';

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
      return api.post<IUserInfoModel, AxiosResponse<IUserInfoModel>, any>(`/auth/userinfo`);
    },
    requestCode: (model: IRegisterModel) => {
      return api.put<IRegisterModel, AxiosResponse<IRegisterModel>, any>(
        `/auth/request/code`,
        model,
      );
    },
    requestApproval: (model: IUserModel) => {
      return api.put<IUserModel, AxiosResponse<IUserModel>, any>(`/auth/request/approval`, model);
    },
  }).current;
};
