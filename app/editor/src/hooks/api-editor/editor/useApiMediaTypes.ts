import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IMediaTypeModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiMediaTypes = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getMediaTypes: () => {
      return api.get<IMediaTypeModel[]>(`/editor/media/types`);
    },
  };
};
