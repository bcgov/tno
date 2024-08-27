import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import {
  IPaged,
  ITransferAccount,
  IUserAVOverviewModel,
  IUserFilter,
  IUserModel,
  IUserNotificationModel,
  IUserProductModel,
  IUserReportModel,
  useApi,
} from '..';

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
      return api.get<never, AxiosResponse<IPaged<IUserModel>>, any>(
        `/admin/users?${toQueryString(filter)}`,
      );
    },
    getUser: (id: number) => {
      return api.get<never, AxiosResponse<IUserModel>, any>(`/admin/users/${id}`);
    },
    addUser: (model: IUserModel) => {
      return api.post<IUserModel, AxiosResponse<IUserModel>, any>(`/admin/users`, model);
    },
    updateUser: (model: IUserModel) => {
      return api.put<IUserModel, AxiosResponse<IUserModel>, any>(`/admin/users/${model.id}`, model);
    },
    deleteUser: (model: IUserModel) => {
      return api.delete<IUserModel, AxiosResponse<IUserModel>, any>(`/admin/users/${model.id}`, {
        data: model,
      });
    },
    transferAccount: (model: ITransferAccount) => {
      return api.post<ITransferAccount, AxiosResponse<IUserModel>, any>(
        `/admin/users/transfer`,
        model,
      );
    },
    getDistributionListById: (id: number) => {
      return api.get<never, AxiosResponse<IUserModel[]>, any>(`/admin/users/${id}/distribution`);
    },
    getUserProductSubscriptions: (id: number) => {
      return api.get<never, AxiosResponse<IUserProductModel[]>, any>(
        `/admin/users/${id}/product/subscriptions`,
      );
    },
    getUserReportSubscriptions: (id: number) => {
      return api.get<never, AxiosResponse<IUserReportModel[]>, any>(
        `/admin/users/${id}/report/subscriptions`,
      );
    },
    getUserEveningOverviewSubscriptions: (id: number) => {
      return api.get<never, AxiosResponse<IUserAVOverviewModel[]>, any>(
        `/admin/users/${id}/evening/overview/subscriptions`,
      );
    },
    getUserNotificationSubscriptions: (id: number) => {
      return api.get<never, AxiosResponse<IUserNotificationModel[]>, any>(
        `/admin/users/${id}/notification/subscriptions`,
      );
    },
  }).current;
};
