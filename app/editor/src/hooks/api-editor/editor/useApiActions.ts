import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { IActionModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiActions = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getActions: () => {
      return extractResponseData<IActionModel[]>(() => api.get(`/editor/actions`));
    },
  };
};
