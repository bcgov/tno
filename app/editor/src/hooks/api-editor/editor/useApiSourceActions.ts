import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { ISourceActionModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiSourceActions = (
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
      return extractResponseData<ISourceActionModel[]>(() => api.get(`/editor/source/actions`));
    },
  };
};
