import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IFilterModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminFilters = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllFilters: () => {
      return api.get<never, AxiosResponse<IFilterModel[]>, any>(`/admin/filters`);
    },
    getFilter: (id: number) => {
      return api.get<never, AxiosResponse<IFilterModel>, any>(`/admin/filters/${id}`);
    },
    addFilter: (model: IFilterModel) => {
      return api.post<IFilterModel, AxiosResponse<IFilterModel>, any>(`/admin/filters`, model);
    },
    updateFilter: (model: IFilterModel) => {
      return api.put<IFilterModel, AxiosResponse<IFilterModel>, any>(
        `/admin/filters/${model.id}`,
        model,
      );
    },
    deleteFilter: (model: IFilterModel) => {
      return api.delete<IFilterModel, AxiosResponse<IFilterModel>, any>(
        `/admin/filters/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
