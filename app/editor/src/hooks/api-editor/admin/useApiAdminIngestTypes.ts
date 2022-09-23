import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IIngestTypeFilter, IIngestTypeModel, IPaged, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminIngestTypes = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllIngestTypes: () => {
      return api.get<IIngestTypeModel[], AxiosResponse<IIngestTypeModel[], never>, any>(
        `/admin/ingest/types/all`,
      );
    },
    findIngestTypes: (filter: IIngestTypeFilter) => {
      return api.get<IPaged<IIngestTypeModel>, AxiosResponse<IPaged<IIngestTypeModel>, never>, any>(
        `/admin/ingest/types?${toQueryString(filter)}`,
      );
    },
    getIngestType: (id: number) => {
      return api.get<IIngestTypeModel, AxiosResponse<IIngestTypeModel, never>, any>(
        `/admin/ingest/types/${id}`,
      );
    },
    addIngestType: (model: IIngestTypeModel) => {
      return api.post<IIngestTypeModel, AxiosResponse<IIngestTypeModel, never>, any>(
        `/admin/ingest/types`,
        model,
      );
    },
    updateIngestType: (model: IIngestTypeModel) => {
      return api.put<IIngestTypeModel, AxiosResponse<IIngestTypeModel, never>, any>(
        `/admin/ingest/types/${model.id}`,
        model,
      );
    },
    deleteIngestType: (model: IIngestTypeModel) => {
      return api.delete<IIngestTypeModel, AxiosResponse<IIngestTypeModel, never>, any>(
        `/admin/ingest/types/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
