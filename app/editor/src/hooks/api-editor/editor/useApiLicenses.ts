import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ILicenseModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiLicenses = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getLicenses: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<ILicenseModel[], AxiosResponse<ILicenseModel[]>, any>(
        `/editor/licenses`,
        config,
      );
    },
  }).current;
};
