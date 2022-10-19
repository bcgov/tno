import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IContentModel, IPaged, IWorkOrderFilter, IWorkOrderModel, useApi } from '..';

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
      return api.get<IPaged<IWorkOrderModel>, AxiosResponse<IPaged<IWorkOrderModel>>, any>(
        `/editor/work/orders?${toQueryString(filter)}`,
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
