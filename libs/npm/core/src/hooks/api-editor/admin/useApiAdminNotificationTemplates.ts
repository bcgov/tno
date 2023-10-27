import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { INotificationTemplateModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminNotificationTemplates = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllNotificationTemplates: () => {
      return api.get<never, AxiosResponse<INotificationTemplateModel[]>, any>(
        `/admin/notification/templates`,
      );
    },
    getNotificationTemplate: (id: number) => {
      return api.get<never, AxiosResponse<INotificationTemplateModel>, any>(
        `/admin/notification/templates/${id}`,
      );
    },
    addNotificationTemplate: (model: INotificationTemplateModel) => {
      return api.post<INotificationTemplateModel, AxiosResponse<INotificationTemplateModel>, any>(
        `/admin/notification/templates`,
        model,
      );
    },
    updateNotificationTemplate: (model: INotificationTemplateModel) => {
      return api.put<INotificationTemplateModel, AxiosResponse<INotificationTemplateModel>, any>(
        `/admin/notification/templates/${model.id}`,
        model,
      );
    },
    deleteNotificationTemplate: (model: INotificationTemplateModel) => {
      return api.delete<INotificationTemplateModel, AxiosResponse<INotificationTemplateModel>, any>(
        `/admin/notification/templates/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
