import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IPaged, ISourceModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminSources = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllSources: () => {
      return api.get<ISourceModel[], AxiosResponse<ISourceModel[]>, any>(`/admin/sources`);
    },
    findSources: () => {
      return api.get<IPaged<ISourceModel>, AxiosResponse<IPaged<ISourceModel>>, any>(
        `/admin/sources/find`,
      );
    },
    getSource: (id: number) => {
      return api.get<ISourceModel, AxiosResponse<ISourceModel>, any>(`/admin/sources/${id}`);
    },
    addSource: (model: ISourceModel) => {
      return api.post<ISourceModel, AxiosResponse<ISourceModel>, any>(`/admin/sources`, model);
    },
    updateSource: (model: ISourceModel) => {
      return api.put<ISourceModel, AxiosResponse<ISourceModel>, any>(
        `/admin/sources/${model.id}`,
        model,
      );
    },
    deleteSource: (model: ISourceModel) => {
      return api.delete<ISourceModel, AxiosResponse<ISourceModel>, any>(
        `/admin/sources/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
