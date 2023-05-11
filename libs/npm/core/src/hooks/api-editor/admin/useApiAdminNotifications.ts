import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { INotificationModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminNotifications = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllNotifications: () => {
      return api.get<INotificationModel[], AxiosResponse<INotificationModel[]>, any>(
        `/admin/notifications`,
      );
    },
    getNotification: (id: number) => {
      return api.get<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications/${id}`,
      );
    },
    addNotification: (model: INotificationModel) => {
      return api.post<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications`,
        model,
      );
    },
    updateNotification: (model: INotificationModel) => {
      return api.put<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications/${model.id}`,
        model,
      );
    },
    deleteNotification: (model: INotificationModel) => {
      return api.delete<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications/${model.id}`,
        {
          data: model,
        },
      );
    },
    sendNotification: (model: INotificationModel, to: string) => {
      return api.post<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications/${model.id}/send?to=${to}`,
      );
    },
  }).current;
};
