import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IConnectionModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminConnections = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllConnections: () => {
      return api.get<IConnectionModel[], AxiosResponse<IConnectionModel[]>, any>(
        `/admin/connections`,
      );
    },
    getConnection: (id: number) => {
      return api.get<IConnectionModel, AxiosResponse<IConnectionModel>, any>(
        `/admin/connections/${id}`,
      );
    },
    addConnection: (model: IConnectionModel) => {
      return api.post<IConnectionModel, AxiosResponse<IConnectionModel>, any>(
        `/admin/connections`,
        model,
      );
    },
    updateConnection: (model: IConnectionModel) => {
      return api.put<IConnectionModel, AxiosResponse<IConnectionModel>, any>(
        `/admin/connections/${model.id}`,
        model,
      );
    },
    deleteConnection: (model: IConnectionModel) => {
      return api.delete<IConnectionModel, AxiosResponse<IConnectionModel>, any>(
        `/admin/connections/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
