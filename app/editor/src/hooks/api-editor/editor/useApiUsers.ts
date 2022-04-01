import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IUserModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiUsers = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getUsers: () => {
      return api.get<IUserModel[]>(`/editor/users`);
    },
  };
};
