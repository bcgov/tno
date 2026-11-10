import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IOrganizationModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminOrganizations = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllOrganizations: () => {
      return api.get<never, AxiosResponse<IOrganizationModel[]>, any>(`/admin/organizations`);
    },
    getOrganization: (id: number) => {
      return api.get<never, AxiosResponse<IOrganizationModel>, any>(`/admin/organizations/${id}`);
    },
    addOrganization: (model: IOrganizationModel) => {
      return api.post<IOrganizationModel, AxiosResponse<IOrganizationModel>, any>(
        `/admin/organizations`,
        model,
      );
    },
    updateOrganization: (model: IOrganizationModel) => {
      return api.put<IOrganizationModel, AxiosResponse<IOrganizationModel>, any>(
        `/admin/organizations/${model.id}`,
        model,
      );
    },
    deleteOrganization: (model: IOrganizationModel) => {
      return api.delete<IOrganizationModel, AxiosResponse<IOrganizationModel>, any>(
        `/admin/organizations/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
