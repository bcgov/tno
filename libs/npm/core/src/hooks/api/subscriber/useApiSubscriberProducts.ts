import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, IProductSubscriberModel, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberProducts = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getProducts: () => {
      return api.get<never, AxiosResponse<IProductSubscriberModel[]>, any>(`/subscriber/products`);
    },
    toggleSubscription: (model: IProductSubscriberModel) => {
      return api.put<IProductSubscriberModel, AxiosResponse<IProductSubscriberModel>, any>(
        `/subscriber/products/${model.id}/togglesubscription`,
        model,
      );
    },
  }).current;
};
