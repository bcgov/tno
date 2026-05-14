import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts, ILLMFilter, ILLMModel, useApi } from '../..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberLLMs = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findLLMs: (filter?: ILLMFilter) => {
      return api.get<never, AxiosResponse<ILLMModel[]>, any>(
        `/subscriber/llms?${toQueryString(filter ?? {})}`,
      );
    },
    getLLMs: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<never, AxiosResponse<ILLMModel[]>, any>(`/subscriber/llms`, config);
    },
  }).current;
};
