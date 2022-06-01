import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IPaged, IUserFilter, IUserModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminUsers = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findUsers: (filter: IUserFilter) => {
      return api.get<IPaged<IUserModel>, AxiosResponse<IPaged<IUserModel>, never>, any>(
        `/admin/users?${toQueryString(filter)}`,
      );
    },
    getUser: (id: number) => {
      return api.get<IUserModel, AxiosResponse<IUserModel, never>, any>(`/admin/users/${id}`);
    },
    addUser: (model: IUserModel) => {
      return api.post<IUserModel, AxiosResponse<IUserModel, never>, any>(`/admin/users`, model);
    },
    updateUser: (model: IUserModel) => {
      return api.put<IUserModel, AxiosResponse<IUserModel, never>, any>(
        `/admin/users/${model.id}`,
        model,
      );
    },
    deleteUser: (model: IUserModel) => {
      return api.delete<IUserModel, AxiosResponse<IUserModel, never>, any>(
        `/admin/users/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
