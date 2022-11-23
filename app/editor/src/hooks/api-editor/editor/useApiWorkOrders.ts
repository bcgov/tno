import { AxiosResponse } from 'axios';
import React from 'react';
import { IWorkOrderFilter, IWorkOrderModel } from 'store/slices/workorder/interfaces';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IContentModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiWorkOrders = (
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
      console.log('here');
      return api.get<IPaged<IWorkOrderModel>, AxiosResponse<IPaged<IWorkOrderModel>>, any>(
        `/editor/work/orders?${toQueryString(filter)}`,
      );
    },
    getWorkOrder: (id: number) => {
      return api.get<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders/${id}`,
      );
    },
    addWorkOrder: (model: IWorkOrderModel) => {
      return api.post<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders`,
        model,
      );
    },
    updateWorkOrder: (model: IWorkOrderModel) => {
      return api.put<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders/${model.id}`,
        model,
      );
    },
    deleteWorkOrder: (model: IWorkOrderModel) => {
      return api.delete<IWorkOrderModel, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders/${model.id}`,
        {
          data: model,
        },
      );
    },
    transcribe: (content: IContentModel) => {
      return api.put<never, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders/transcribe/${content.id}`,
      );
    },
    nlp: (content: IContentModel) => {
      return api.put<never, AxiosResponse<IWorkOrderModel>, any>(
        `/editor/work/orders/nlp/${content.id}`,
      );
    },
  }).current;
};
