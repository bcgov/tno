import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IFilterModel, ILifecycleToasts, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberFilters = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findMyFilters: () => {
      return api.get<IFilterModel[], AxiosResponse<IFilterModel[]>, any>(
        `/subscriber/filters/my-filters`,
      );
    },
    getFilter: (id: number) => {
      return api.get<IFilterModel, AxiosResponse<IFilterModel>, any>(`/subscriber/filters/${id}`);
    },
    addFilter: (model: IFilterModel) => {
      return api.post<IFilterModel, AxiosResponse<IFilterModel>, any>(`/subscriber/filters`, model);
    },
    updateFilter: (model: IFilterModel) => {
      return api.put<IFilterModel, AxiosResponse<IFilterModel>, any>(
        `/subscriber/filters/${model.id}`,
        model,
      );
    },
    deleteFilter: (model: IFilterModel) => {
      return api.delete<IFilterModel, AxiosResponse<IFilterModel>, any>(
        `/subscriber/filters/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
