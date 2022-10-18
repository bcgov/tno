import { AxiosResponse } from 'axios';
import React from 'react';
import { defaultEnvelope, ILifecycleToasts, toQueryString } from 'tno-core';

import { IPaged, ISeriesModel, useApi } from '..';
import { ITagFilter } from '../interfaces/ITagFilter';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminSeries = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllSeries: () => {
      return api.get<ISeriesModel[], AxiosResponse<ISeriesModel[], never>, any>(
        `/admin/series/all`,
      );
    },
    findSeries: (filter: ITagFilter) => {
      return api.get<IPaged<ISeriesModel>, AxiosResponse<IPaged<ISeriesModel>, never>, any>(
        `/admin/Series?${toQueryString(filter)}`,
      );
    },
    getSeries: (id: number) => {
      return api.get<ISeriesModel, AxiosResponse<ISeriesModel, never>, any>(`/admin/series/${id}`);
    },
    addSeries: (model: ISeriesModel) => {
      return api.post<ISeriesModel, AxiosResponse<ISeriesModel, never>, any>(
        `/admin/series`,
        model,
      );
    },
    updateSeries: (model: ISeriesModel) => {
      return api.put<ISeriesModel, AxiosResponse<ISeriesModel, never>, any>(
        `/admin/series/${model.id}`,
        model,
      );
    },
    deleteSeries: (model: ISeriesModel) => {
      return api.delete<ISeriesModel, AxiosResponse<ISeriesModel, never>, any>(
        `/admin/series/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
