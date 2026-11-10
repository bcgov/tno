import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { ICacheModel, useApi } from '..';

/**
 * Common hook to make requests to the APi.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorCache = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getCache: () => {
      return api.get<ICacheModel[], AxiosResponse<ICacheModel[]>, any>(`/editor/cache`);
    },
  }).current;
};
