import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IActionFilter, IActionModel, IPaged, useApi } from '..';
/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminActions = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllActions: () => {
      return api.get<never, AxiosResponse<IActionModel[]>, any>(`/admin/actions`);
    },
    findActions: (filter: IActionFilter) => {
      return api.get<never, AxiosResponse<IPaged<IActionModel>>, any>(
        `/admin/actions?${toQueryString(filter)}`,
      );
    },
    getAction: (id: number) => {
      return api.get<never, AxiosResponse<IActionModel>, any>(`/admin/actions/${id}`);
    },
    addAction: (model: IActionModel) => {
      return api.post<IActionModel, AxiosResponse<IActionModel>, any>(`/admin/actions`, model);
    },
    updateAction: (model: IActionModel) => {
      return api.put<IActionModel, AxiosResponse<IActionModel>, any>(
        `/admin/actions/${model.id}`,
        model,
      );
    },
    deleteAction: (model: IActionModel) => {
      return api.delete<IActionModel, AxiosResponse<IActionModel>, any>(
        `/admin/actions/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
