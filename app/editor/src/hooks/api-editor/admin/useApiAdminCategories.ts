import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { ICategoryModel, IPaged, useApi } from '..';
import { ICategoryFilter } from '../interfaces/ICategoryFilter';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminCategories = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllCategories: () => {
      return api.get<ICategoryModel[], AxiosResponse<ICategoryModel[], never>, any>(
        `/admin/categories/all`,
      );
    },
    findCategories: (filter: ICategoryFilter) => {
      return api.get<IPaged<ICategoryModel>, AxiosResponse<IPaged<ICategoryModel>, never>, any>(
        `/admin/categories?${toQueryString(filter)}`,
      );
    },
    getCategory: (id: number) => {
      return api.get<ICategoryModel, AxiosResponse<ICategoryModel, never>, any>(
        `/admin/categories/${id}`,
      );
    },
    addCategory: (model: ICategoryModel) => {
      return api.post<ICategoryModel, AxiosResponse<ICategoryModel, never>, any>(
        `/admin/categories`,
        model,
      );
    },
    updateCategory: (model: ICategoryModel) => {
      return api.put<ICategoryModel, AxiosResponse<ICategoryModel, never>, any>(
        `/admin/categories/${model.id}`,
        model,
      );
    },
    deleteCategory: (model: ICategoryModel) => {
      return api.delete<ICategoryModel, AxiosResponse<ICategoryModel, never>, any>(
        `/admin/categories/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
