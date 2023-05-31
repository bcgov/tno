import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IReportModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiAdminReports = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    findAllReports: () => {
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(`/admin/reports`);
    },
    getReport: (id: number, includeInstances: boolean) => {
      return api.get<IReportModel, AxiosResponse<IReportModel>, any>(
        `/admin/reports/${id}?includeInstances=${includeInstances}`,
      );
    },
    addReport: (model: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportModel>, any>(`/admin/reports`, model);
    },
    updateReport: (model: IReportModel) => {
      return api.put<IReportModel, AxiosResponse<IReportModel>, any>(
        `/admin/reports/${model.id}`,
        model,
      );
    },
    deleteReport: (model: IReportModel) => {
      return api.delete<IReportModel, AxiosResponse<IReportModel>, any>(
        `/admin/reports/${model.id}`,
        {
          data: model,
        },
      );
    },
    sendReport: (model: IReportModel, to: string) => {
      return api.post<IReportModel, AxiosResponse<IReportModel>, any>(
        `/admin/reports/${model.id}/send?to=${to}`,
      );
    },
    publishReport: (model: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportModel>, any>(
        `/admin/reports/${model.id}/publish`,
      );
    },
  }).current;
};
