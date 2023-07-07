import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { ISystemMessageModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminSystemMessages = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findSystemMessage: () => {
      return api.get<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-message`,
      );
    },
    addSystemMessage: (model: ISystemMessageModel) => {
      return api.post<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-message`,
        model,
      );
    },
    updateSystemMessage: (model: ISystemMessageModel) => {
      return api.put<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-message/${model.id}`,
        model,
      );
    },
    deleteSystemMessage: (model: ISystemMessageModel) => {
      return api.delete<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-message/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
