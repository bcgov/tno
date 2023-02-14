import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IIngestFilter, IIngestModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiIngests = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findIngests: (filter?: IIngestFilter) => {
      const params = {
        ...filter,
        ingestTypeId: filter?.ingestTypeId?.length ? filter.ingestTypeId : undefined,
      };
      return api.get<IPaged<IIngestModel>, AxiosResponse<IPaged<IIngestModel>>, any>(
        `/editor/ingests/find?${toQueryString(params)}`,
      );
    },
  }).current;
};
