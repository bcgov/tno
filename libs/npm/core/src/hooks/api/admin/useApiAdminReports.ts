import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import {
  IReportDashboard,
  IReportFilter,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  useApi,
} from '..';

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
    findReports: (filter?: IReportFilter) => {
      var query = toQueryString(filter ?? {});
      return api.get<never, AxiosResponse<IReportModel[]>, any>(`/admin/reports?${query}`);
    },
    findAllReportsHeadersOnly: () => {
      return api.get<never, AxiosResponse<IReportModel[]>, any>(`/admin/reports/headers`);
    },
    findInstancesForReportId: (id: number, ownerId: number | undefined = undefined) => {
      return api.get<never, AxiosResponse<IReportInstanceModel[]>, any>(
        `/admin/reports/${id}/instances?ownerId=${ownerId ? ownerId : ''}`,
      );
    },
    getReport: (id: number) => {
      return api.get<never, AxiosResponse<IReportModel>, any>(`/admin/reports/${id}`);
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
    previewReport: (model: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportResultModel>, any>(
        `/admin/reports/preview`,
        model,
      );
    },
    primeReportCache: (model: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportResultModel>, any>(
        `/admin/reports/preview/prime`,
        model,
      );
    },
    getDashboard: () => {
      return api.get<never, AxiosResponse<IReportDashboard>, any>(`/admin/reports/dashboard`);
    },
  }).current;
};
