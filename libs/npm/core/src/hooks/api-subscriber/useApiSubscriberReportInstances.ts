import { AxiosResponse } from 'axios';
import React from 'react';

import {
  defaultEnvelope,
  ILifecycleToasts,
  IReportInstanceModel,
  IReportResultModel,
  useApi,
  useDownload,
} from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberReportInstances = (
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
    getReportInstance: (id: number) => {
      return api.get<never, AxiosResponse<IReportInstanceModel | undefined>, any>(
        `/subscriber/report/instances/${id}`,
      );
    },
    addReportInstance: (report: IReportInstanceModel) => {
      return api.post<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        '/subscriber/report/instances',
        report,
      );
    },
    updateReportInstance: (report: IReportInstanceModel) => {
      return api.put<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${report.id}`,
        report,
      );
    },
    deleteReportInstance: (report: IReportInstanceModel) => {
      return api.delete<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${report.id}`,
        { data: report },
      );
    },
    viewReportInstance: (instanceId: number) => {
      return api.post<never, AxiosResponse<IReportResultModel>, any>(
        `/subscriber/report/instances/${instanceId}/view`,
      );
    },
    exportReport: (reportId: number, reportName: string) => {
      return download({
        url: `/subscriber/report/instances/${reportId}/export`,
        method: 'post',
        fileName: `${reportName}.xlsx`,
      });
    },
    sendReportInstance: (reportId: number, to: string) => {
      return api.post<never, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${reportId}/send?to=${to}`,
      );
    },
    publishReportInstance: (reportId: number) => {
      return api.post<never, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${reportId}/publish`,
      );
    },
  }).current;
};
