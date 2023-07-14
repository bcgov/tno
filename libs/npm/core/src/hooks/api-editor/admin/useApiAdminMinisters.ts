import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IMinisterModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminMinisters = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllMinisters: () => {
      return api.get<IMinisterModel[], AxiosResponse<IMinisterModel[]>, any>(`/admin/ministers`);
    },
    getMinister: (id: number) => {
      return api.get<IMinisterModel, AxiosResponse<IMinisterModel>, any>(`/admin/ministers/${id}`);
    },
    addMinister: (model: IMinisterModel) => {
      return api.post<IMinisterModel, AxiosResponse<IMinisterModel>, any>(
        `/admin/ministers`,
        model,
      );
    },
    updateMinister: (model: IMinisterModel) => {
      return api.put<IMinisterModel, AxiosResponse<IMinisterModel>, any>(
        `/admin/ministers/${model.id}`,
        model,
      );
    },
    deleteMinister: (model: IMinisterModel) => {
      return api.delete<IMinisterModel, AxiosResponse<IMinisterModel>, any>(
        `/admin/ministers/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
