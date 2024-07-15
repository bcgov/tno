import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IProductModel, useApi } from '..';
import { IProductFilter } from '../interfaces/IProductFilter';

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
    findProducts: (filter: IProductFilter) => {
      var query = toQueryString(filter);
      return api.get<never, AxiosResponse<IProductModel[]>, any>(`/admin/products?${query}`);
    },
    getProduct: (id: number) => {
      return api.get<never, AxiosResponse<IProductModel>, any>(`/admin/products/${id}`);
    },
    addProduct: (model: IProductModel) => {
      return api.post<IProductModel, AxiosResponse<IProductModel>, any>(`/admin/products`, model);
    },
    updateProduct: (model: IProductModel) => {
      return api.put<IProductModel, AxiosResponse<IProductModel>, any>(
        `/admin/products/${model.id}`,
        model,
      );
    },
    deleteProduct: (model: IProductModel) => {
      return api.delete<IProductModel, AxiosResponse<IProductModel>, any>(
        `/admin/products/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
