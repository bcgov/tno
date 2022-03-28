import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { ITagModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiTags = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getTags: () => {
      return extractResponseData<ITagModel[]>(() => api.get(`/editor/tags`));
    },
  };
};
