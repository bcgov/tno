import React from 'react';
import { defaultEnvelope, extractResponseData, LifecycleToasts } from 'tno-core';

import { IActionModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiActions = (
  options: {
    lifecycleToasts?: LifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useMemo(
    () => ({
      // Lookups
      getActions: () => {
        return extractResponseData<IActionModel[]>(() => api.get(`/editor/actions`));
      },
    }),
    [api],
  );
};
