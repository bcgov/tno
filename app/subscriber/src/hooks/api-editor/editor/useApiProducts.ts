import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IProductModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiProducts = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getProducts: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IProductModel[], AxiosResponse<IProductModel[]>, any>(
        `/editor/products`,
        config,
      );
    },
  }).current;
};
