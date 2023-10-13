import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IDataLocationModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminDataLocations = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllDataLocations: () => {
      return api.get<never, AxiosResponse<IDataLocationModel[]>, any>(`/admin/data/locations`);
    },
    getDataLocation: (id: number) => {
      return api.get<never, AxiosResponse<IDataLocationModel>, any>(`/admin/data/locations/${id}`);
    },
    addDataLocation: (model: IDataLocationModel) => {
      return api.post<IDataLocationModel, AxiosResponse<IDataLocationModel>, any>(
        `/admin/data/locations`,
        model,
      );
    },
    updateDataLocation: (model: IDataLocationModel) => {
      return api.put<IDataLocationModel, AxiosResponse<IDataLocationModel>, any>(
        `/admin/data/locations/${model.id}`,
        model,
      );
    },
    deleteDataLocation: (model: IDataLocationModel) => {
      return api.delete<IDataLocationModel, AxiosResponse<IDataLocationModel>, any>(
        `/admin/data/locations/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
