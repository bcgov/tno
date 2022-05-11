import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { useApi } from '..';
import { IClaimModel } from '../interfaces';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiClaims = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getClaims: (etag: string | undefined = undefined) => {
      const config = { headers: { 'If-None-Match': etag ?? '' } };
      return api.get<IClaimModel[]>(`/editor/claims`, config);
    },
  };
};
