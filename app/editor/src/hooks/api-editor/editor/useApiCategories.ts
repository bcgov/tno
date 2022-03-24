import React from 'react';
import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { IContentTypeModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiCategories = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getCategories: () => {
      return extractResponseData<IContentTypeModel[]>(() => api.get(`/editor/categories`));
    },
  }).current;
};
