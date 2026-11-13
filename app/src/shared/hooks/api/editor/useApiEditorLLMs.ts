import { AxiosResponse } from 'axios';
import React from 'react';

import { ILLMFilter, ILLMModel, useApi } from '..';
import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorLLMs = (
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
        `/editor/llms?${toQueryString(filter ?? {})}`,
      );
    },
    getLLMs: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<never, AxiosResponse<ILLMModel[]>, any>(`/editor/llms`, config);
    },
  }).current;
};
