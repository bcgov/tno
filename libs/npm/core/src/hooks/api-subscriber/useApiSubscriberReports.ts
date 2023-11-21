import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../utils';
import {
  defaultEnvelope,
  ILifecycleToasts,
  IReportFilter,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  useApi,
  useDownload,
} from '..';

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
  const download = useDownload(api);

  return React.useRef({
    findReports: (filter?: IReportFilter) => {
      const query = { ...filter };
      return api.get<never, AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports?${toQueryString(query)}`,
      );
    },
    findInstancesForReportId: (id: number, ownerId: number | undefined = undefined) => {
      return api.get<never, AxiosResponse<IReportInstanceModel[]>, any>(
        `/subscriber/reports/${id}/instances?ownerId=${ownerId ? ownerId : ''}`,
      );
    },
    findMyReports: () => {
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports/my-reports`,
      );
    },
    getPublicReports: () => {
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports/public`,
      );
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
    updateReport: (report: IReportModel, updateInstances: boolean | undefined) => {
      return api.put<IReportModel, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${report.id}${
          updateInstances !== undefined ? `?updateInstances=${updateInstances}` : ''
        }`,
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
    generateReport: (reportId: number, regenerate: boolean | undefined = false) => {
      return api.post<never, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${reportId}/generate${
          regenerate !== undefined ? `?regenerate=${regenerate}` : ''
        }`,
      );
    },
    exportReport: (reportId: number) => {
      return download({
        url: `/subscriber/reports/${reportId}/export`,
        method: 'post',
        fileName: `myreport${reportId}.xlsx`,
      });
    },
    sendReport: (reportId: number, to: string) => {
      return api.post<never, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${reportId}/send?to=${to}`,
      );
    },
    publishReport: (reportId: number) => {
      return api.post<never, AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${reportId}/publish`,
      );
    },
  }).current;
};
