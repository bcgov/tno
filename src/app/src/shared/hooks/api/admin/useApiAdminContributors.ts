import { AxiosResponse } from 'axios';
import React from 'react';

import { IContributorModel, useApi } from '..';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminContributors = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllContributors: () => {
      return api.get<never, AxiosResponse<IContributorModel[]>, any>(`/admin/contributors/all`);
    },
    getContributor: (id: number) => {
      return api.get<never, AxiosResponse<IContributorModel>, any>(`/admin/contributors/${id}`);
    },
    addContributor: (model: IContributorModel) => {
      return api.post<IContributorModel, AxiosResponse<IContributorModel>, any>(
        `/admin/contributors`,
        model,
      );
    },
    updateContributor: (model: IContributorModel) => {
      return api.put<IContributorModel, AxiosResponse<IContributorModel>, any>(
        `/admin/contributors/${model.id}`,
        model,
      );
    },
    deleteContributor: (model: IContributorModel) => {
      return api.delete<IContributorModel, AxiosResponse<IContributorModel>, any>(
        `/admin/contributors/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
