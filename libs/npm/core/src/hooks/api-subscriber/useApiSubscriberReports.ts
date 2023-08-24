import { AxiosResponse } from 'axios';
import React from 'react';

import {
  ILifecycleToasts,
  IReportFilter,
  IReportModel,
  IReportResultModel,
  defaultEnvelope,
  useApi,
} from '..';
import { toQueryString } from 'tno-core';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberReports = (
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
      const query = { ...filter };
      return api.get<never, AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports?${toQueryString(query)}`,
      );
    },
    findMyReports: () => {
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports/my-reports`,
      );
    },
    findAllReports: () => {
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(`/subscriber/reports/all`);
    },
    getReport: (id: number) => {
      return api.get<never, AxiosResponse<IReportModel | undefined>, any>(
        `/subscriber/reports/${id}`,
      );
    },
    addReport: (report: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportModel>, any>(
        '/subscriber/reports',
        report,
      );
    },
    updateReport: (report: IReportModel) => {
      return api.put<IReportModel, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${report.id}`,
        report,
      );
    },
    deleteReport: (report: IReportModel) => {
      return api.delete<IReportModel, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${report.id}`,
        { data: report },
      );
    },
    previewReport: (reportId: number) => {
      return api.post<never, AxiosResponse<IReportResultModel>, any>(
        `/subscriber/reports/${reportId}/preview`,
      );
    },
  }).current;
};
