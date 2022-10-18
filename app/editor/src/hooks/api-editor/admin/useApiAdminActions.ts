import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

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
      return api.get<IActionModel[], AxiosResponse<IActionModel[], never>, any>(`/admin/actions`);
    },
    findActions: (filter: IActionFilter) => {
      return api.get<IPaged<IActionModel>, AxiosResponse<IPaged<IActionModel>, never>, any>(
        `/admin/actions?${toQueryString(filter)}`,
      );
    },
    getAction: (id: number) => {
      return api.get<IActionModel, AxiosResponse<IActionModel, never>, any>(`/admin/actions/${id}`);
    },
    addAction: (model: IActionModel) => {
      return api.post<IActionModel, AxiosResponse<IActionModel, never>, any>(
        `/admin/actions`,
        model,
      );
    },
    updateAction: (model: IActionModel) => {
      return api.put<IActionModel, AxiosResponse<IActionModel, never>, any>(
        `/admin/actions/${model.id}`,
        model,
      );
    },
    deleteAction: (model: IActionModel) => {
      return api.delete<IActionModel, AxiosResponse<IActionModel, never>, any>(
        `/admin/actions/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
