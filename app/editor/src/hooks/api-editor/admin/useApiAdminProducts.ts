import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IProductModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminProducts = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllProducts: () => {
      return api.get<IProductModel[], AxiosResponse<IProductModel[], never>, any>(
        `/admin/products`,
      );
    },
    getProduct: (id: number) => {
      return api.get<IProductModel, AxiosResponse<IProductModel, never>, any>(
        `/admin/products/${id}`,
      );
    },
    addProduct: (model: IProductModel) => {
      return api.post<IProductModel, AxiosResponse<IProductModel, never>, any>(
        `/admin/products`,
        model,
      );
    },
    updateProduct: (model: IProductModel) => {
      return api.put<IProductModel, AxiosResponse<IProductModel, never>, any>(
        `/admin/products/${model.id}`,
        model,
      );
    },
    deleteProduct: (model: IProductModel) => {
      return api.delete<IProductModel, AxiosResponse<IProductModel, never>, any>(
        `/admin/products/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
