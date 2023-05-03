import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IAlertModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminAlerts = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllAlerts: () => {
      return api.get<IAlertModel[], AxiosResponse<IAlertModel[]>, any>(`/admin/alerts/all`);
    },
    getAlert: (id: number) => {
      return api.get<IAlertModel, AxiosResponse<IAlertModel>, any>(`/admin/alerts/${id}`);
    },
    addAlert: (model: IAlertModel) => {
      return api.post<IAlertModel, AxiosResponse<IAlertModel>, any>(`/admin/alerts`, model);
    },
    updateAlert: (model: IAlertModel) => {
      return api.put<IAlertModel, AxiosResponse<IAlertModel>, any>(
        `/admin/alerts/${model.id}`,
        model,
      );
    },
    deleteAlert: (model: IAlertModel) => {
      return api.delete<IAlertModel, AxiosResponse<IAlertModel>, any>(`/admin/alerts/${model.id}`, {
        data: model,
      });
    },
  }).current;
};
