import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts, IProductModel, IUserProductModel, useApi } from '../..';

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
      return api.get<never, AxiosResponse<IProductModel[]>, any>(`/subscriber/products`);
    },
    toggleSubscription: (model: IUserProductModel) => {
      return api.put<IUserProductModel, AxiosResponse<IUserProductModel>, any>(
        `/subscriber/products/${model.productId}/subscription`,
        model,
      );
    },
  }).current;
};
