import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IUserModel, useApi } from '..';
import { IRegisterModel } from '../interfaces';

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
    getUsers: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IUserModel[], AxiosResponse<IUserModel[], never>, any>(
        `/editor/users`,
        config,
      );
    },
    getUser: (id: number) => {
      return api.get<IUserModel, AxiosResponse<IUserModel, never>, any>(`/editor/users/${id}`);
    },
    requestCode: (model: IRegisterModel) => {
      return api.put<IRegisterModel, AxiosResponse<IRegisterModel, never>, any>(
        `/editor/users/request/code`,
        model,
      );
    },
    requestApproval: (model: IUserModel) => {
      return api.put<IUserModel, AxiosResponse<IUserModel, never>, any>(
        `/editor/users/request/approval`,
        model,
      );
    },
  }).current;
};
