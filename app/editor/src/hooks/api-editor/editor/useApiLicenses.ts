import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

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

  return {
    getLicenses: () => {
      return api.get<ILicenseModel[]>(`/editor/licenses`);
    },
  };
};
