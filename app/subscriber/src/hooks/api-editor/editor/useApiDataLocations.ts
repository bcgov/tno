import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IDataLocationModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiDataLocations = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getDataLocations: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IDataLocationModel[], AxiosResponse<IDataLocationModel[]>, any>(
        `/editor/data/locations`,
        config,
      );
    },
    getDataLocation: (id: number) => {
      return api.get<IDataLocationModel, AxiosResponse<IDataLocationModel>, any>(
        `/editor/data/locations/${id}`,
      );
    },
  }).current;
};
