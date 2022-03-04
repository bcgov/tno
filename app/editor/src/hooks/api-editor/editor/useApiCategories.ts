import React from 'react';
import { defaultEnvelope, extractResponseData, LifecycleToasts } from 'tno-core';

import { IContentTypeModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiCategories = (
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
      getCategories: () => {
        return extractResponseData<IContentTypeModel[]>(() => api.get(`/editor/categories`));
      },
    }),
    [api],
  );
};
