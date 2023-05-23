import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, useApi } from 'tno-core';

import { IMinisterModel } from '../interfaces/IMinisterModel';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiMinisters = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getMinisters: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      alert('getMinisters');
      return api.get<IMinisterModel[], AxiosResponse<IMinisterModel[]>, any>(
        `/subscriber/ministers`,
        config,
      );
    },
  }).current;
};
