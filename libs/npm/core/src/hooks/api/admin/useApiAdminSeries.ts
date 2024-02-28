import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IPaged, ISeriesModel, useApi } from '..';
import { ISeriesFilter } from '../interfaces';

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
      return api.get<never, AxiosResponse<ISeriesModel[]>, any>(`/admin/series/all`);
    },
    findSeries: (filter: ISeriesFilter) => {
      return api.get<never, AxiosResponse<IPaged<ISeriesModel>>, any>(
        `/admin/Series?${toQueryString(filter)}`,
      );
    },
    getSeries: (id: number) => {
      return api.get<never, AxiosResponse<ISeriesModel>, any>(`/admin/series/${id}`);
    },
    addSeries: (model: ISeriesModel) => {
      return api.post<ISeriesModel, AxiosResponse<ISeriesModel>, any>(`/admin/series`, model);
    },
    updateSeries: (model: ISeriesModel) => {
      return api.put<ISeriesModel, AxiosResponse<ISeriesModel>, any>(
        `/admin/series/${model.id}`,
        model,
      );
    },
    deleteSeries: (model: ISeriesModel) => {
      return api.delete<ISeriesModel, AxiosResponse<ISeriesModel>, any>(
        `/admin/series/${model.id}`,
        { data: model },
      );
    },
  }).current;
};
