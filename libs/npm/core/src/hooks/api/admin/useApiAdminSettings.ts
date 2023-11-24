import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { ISettingModel, useApi } from '..';
/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminSettings = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllSettings: () => {
      return api.get<never, AxiosResponse<ISettingModel[]>, any>(`/admin/settings`);
    },
    getSetting: (id: number) => {
      return api.get<never, AxiosResponse<ISettingModel>, any>(`/admin/settings/${id}`);
    },
    addSetting: (model: ISettingModel) => {
      return api.post<ISettingModel, AxiosResponse<ISettingModel>, any>(`/admin/settings`, model);
    },
    updateSetting: (model: ISettingModel) => {
      return api.put<ISettingModel, AxiosResponse<ISettingModel>, any>(
        `/admin/settings/${model.id}`,
        model,
      );
    },
    deleteSetting: (model: ISettingModel) => {
      return api.delete<ISettingModel, AxiosResponse<ISettingModel>, any>(
        `/admin/settings/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
