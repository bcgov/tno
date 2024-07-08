import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IFilterFilter, IFilterModel, useApi } from '..';

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
    findFilters: (filter: IFilterFilter) => {
      const query = toQueryString(filter);
      return api.get<never, AxiosResponse<IFilterModel[]>, any>(`/admin/filters?${query}`);
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
