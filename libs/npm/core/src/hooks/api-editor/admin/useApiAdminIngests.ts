import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IIngestModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminIngests = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllIngests: () => {
      return api.get<IIngestModel[], AxiosResponse<IIngestModel[]>, any>(`/admin/ingests`);
    },
    findIngests: () => {
      return api.get<IPaged<IIngestModel>, AxiosResponse<IPaged<IIngestModel>>, any>(
        `/admin/ingests/find`,
      );
    },
    getIngest: (id: number) => {
      return api.get<IIngestModel, AxiosResponse<IIngestModel>, any>(`/admin/ingests/${id}`);
    },
    addIngest: (model: IIngestModel) => {
      return api.post<IIngestModel, AxiosResponse<IIngestModel>, any>(`/admin/ingests`, model);
    },
    updateIngest: (model: IIngestModel) => {
      return api.put<IIngestModel, AxiosResponse<IIngestModel>, any>(
        `/admin/ingests/${model.id}`,
        model,
      );
    },
    deleteIngest: (model: IIngestModel) => {
      return api.delete<IIngestModel, AxiosResponse<IIngestModel>, any>(
        `/admin/ingests/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
