import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { IDataSourceModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiDataSources = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getDataSources: () => {
      return api.get<IDataSourceModel[]>(`/editor/data/sources`);
    },
  };
};
