import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ILicenseModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminLicenses = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllLicenses: () => {
      return api.get<ILicenseModel[], AxiosResponse<ILicenseModel[]>, any>(`/admin/licenses`);
    },
    getLicense: (id: number) => {
      return api.get<ILicenseModel, AxiosResponse<ILicenseModel>, any>(`/admin/licenses/${id}`);
    },
    addLicense: (model: ILicenseModel) => {
      return api.post<ILicenseModel, AxiosResponse<ILicenseModel>, any>(`/admin/licenses`, model);
    },
    updateLicense: (model: ILicenseModel) => {
      return api.put<ILicenseModel, AxiosResponse<ILicenseModel>, any>(
        `/admin/licenses/${model.id}`,
        model,
      );
    },
    deleteLicense: (model: ILicenseModel) => {
      return api.delete<ILicenseModel, AxiosResponse<ILicenseModel>, any>(
        `/admin/licenses/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
