import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { INotificationFilter, INotificationModel, INotificationResultModel, useApi } from '..';

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
    findNotifications: (filter?: INotificationFilter) => {
      var query = toQueryString(filter ?? {});
      return api.get<never, AxiosResponse<INotificationModel[]>, any>(
        `/admin/notifications?${query}`,
      );
    },
    getNotification: (id: number) => {
      return api.get<never, AxiosResponse<INotificationModel>, any>(`/admin/notifications/${id}`);
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
    previewNotification: (model: INotificationModel, contentId: number) => {
      return api.post<INotificationModel, AxiosResponse<INotificationResultModel>, any>(
        `/admin/notifications/${model.id}/preview/${contentId}`,
        model,
      );
    },
    sendNotification: (model: INotificationModel, to: string, contentId?: number) => {
      return api.post<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/admin/notifications/${model.id}/send${contentId ? `/${contentId}` : ''}?to=${to}`,
      );
    },
  }).current;
};
