import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IPaged, IWorkOrderFilter, IWorkOrderModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminWorkOrders = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findWorkOrders: (filter: IWorkOrderFilter) => {
      return api.get<IPaged<IWorkOrderModel>, AxiosResponse<IPaged<IWorkOrderModel>>, any>(
        `/admin/work/orders?${toQueryString(filter)}`,
      );
    },
    getWorkOrder: (id: number) => {
      return api.get<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/admin/work/orders/${id}`,
      );
    },
    addWorkOrder: (model: IWorkOrderModel) => {
      return api.post<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/admin/work/orders`,
        model,
      );
    },
    updateWorkOrder: (model: IWorkOrderModel) => {
      return api.put<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/admin/work/orders/${model.id}`,
        model,
      );
    },
    deleteWorkOrder: (model: IWorkOrderModel) => {
      return api.delete<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/admin/work/orders/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
