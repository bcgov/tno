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
    findSystemMessages: () => {
      return api.get<never, AxiosResponse<ISystemMessageModel[]>, any>(`/admin/system-messages`);
    },
    findSystemMessage: (id: number) => {
      return api.get<never, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-messages/${id}`,
      );
    },
    addSystemMessage: (model: ISystemMessageModel) => {
      return api.post<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-messages`,
        model,
      );
    },
    updateSystemMessage: (model: ISystemMessageModel) => {
      return api.put<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-messages/${model.id}`,
        model,
      );
    },
    deleteSystemMessage: (model: ISystemMessageModel) => {
      return api.delete<ISystemMessageModel, AxiosResponse<ISystemMessageModel>, any>(
        `/admin/system-messages/${model.id}`,
        {
          data: model,
        },
      );
    },
  }).current;
};
