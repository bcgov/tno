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

  return {
    getDataLocations: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IDataLocationModel[]>(`/editor/data/locations`, config);
    },
  };
};
