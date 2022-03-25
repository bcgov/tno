import React from 'react';
import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { ILicenseModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
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
    getLicenses: () => {
      return extractResponseData<ILicenseModel[]>(() => api.get(`/editor/licenses`));
    },
  }).current;
};
