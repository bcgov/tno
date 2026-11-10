import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { IContentModel, IPaged, IWorkOrderFilter, IWorkOrderModel, useApi } from '../..';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberWorkOrders = (
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
        `/subscriber/work/orders?${toQueryString(filter)}`,
      );
    },
    transcribe: (content: IContentModel) => {
      return api.post<never, AxiosResponse<IWorkOrderModel>, any>(
        `/subscriber/work/orders/transcribe/${content.id}`,
      );
    },
    transcribeAnonymous: (contentId: number, uid: number) => {
      return api.get<never, AxiosResponse<any>, any>(
        `/subscriber/work/orders/transcribe/${contentId}?uid=${uid}`,
      );
    },
  }).current;
};
