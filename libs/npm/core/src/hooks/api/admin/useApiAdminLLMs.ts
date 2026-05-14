import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { ILLMModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminLLMs = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllLLMs: () => {
      return api.get<never, AxiosResponse<ILLMModel[]>, any>(`/admin/llms`);
    },
    getLLM: (id: number) => {
      return api.get<never, AxiosResponse<ILLMModel>, any>(`/admin/llms/${id}`);
    },
    addLLM: (model: ILLMModel) => {
      return api.post<ILLMModel, AxiosResponse<ILLMModel>, any>(`/admin/llms`, model);
    },
    updateLLM: (model: ILLMModel) => {
      return api.put<ILLMModel, AxiosResponse<ILLMModel>, any>(`/admin/llms/${model.id}`, model);
    },
    deleteLLM: (model: ILLMModel) => {
      return api.delete<ILLMModel, AxiosResponse<ILLMModel>, any>(`/admin/llms/${model.id}`, {
        data: model,
      });
    },
  }).current;
};
