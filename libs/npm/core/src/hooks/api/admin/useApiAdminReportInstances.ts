import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IReportInstanceModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminReportInstances = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getReportInstance: (id: number) => {
      return api.get<never, AxiosResponse<IReportInstanceModel>, any>(
        `/admin/report/instances/${id}`,
      );
    },
    addReportInstance: (model: IReportInstanceModel) => {
      return api.post<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/admin/report/instances`,
        model,
      );
    },
    updateReportInstance: (model: IReportInstanceModel) => {
      return api.put<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/admin/report/instances/${model.id}`,
        model,
      );
    },
    deleteReportInstance: (model: IReportInstanceModel) => {
      return api.delete<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/admin/report/instances/${model.id}`,
        {
          data: model,
        },
      );
    },
    publishReportInstance: (model: IReportInstanceModel, resend: boolean) => {
      var query = toQueryString({ resend });
      return api.post<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/admin/report/instances/${model.id}/publish?${query}`,
      );
    },
  }).current;
};
