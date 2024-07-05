import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import {
  defaultEnvelope,
  ILifecycleToasts,
  IReportInstanceModel,
  IReportResultModel,
  useApi,
  useDownload,
} from '../..';

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
    getReportInstance: (id: number, includeContent?: boolean, publishedOn?: Date | string) => {
      const query = { publishedOn, includeContent };
      return api.get<never, AxiosResponse<IReportInstanceModel | undefined>, any>(
        `/subscriber/report/instances/${id}?${toQueryString(query)}`,
      );
    },
    addReportInstance: (instance: IReportInstanceModel) => {
      return api.post<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        '/subscriber/report/instances',
        instance,
      );
    },
    updateReportInstance: (instance: IReportInstanceModel) => {
      return api.put<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${instance.id}`,
        instance,
      );
    },
    deleteReportInstance: (instance: IReportInstanceModel) => {
      return api.delete<IReportInstanceModel, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${instance.id}`,
        { data: instance },
      );
    },
    viewReportInstance: (id: number, regenerate: boolean = false) => {
      return api.post<never, AxiosResponse<IReportResultModel>, any>(
        `/subscriber/report/instances/${id}/view?regenerate=${regenerate}`,
      );
    },
    exportReport: (id: number, reportName: string) => {
      return download({
        url: `/subscriber/report/instances/${id}/export`,
        method: 'get',
        fileName: `${reportName}.xlsx`,
      });
    },
    sendReportInstance: (id: number, to: string) => {
      return api.post<never, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${id}/send?to=${to}`,
      );
    },
    publishReportInstance: (id: number) => {
      return api.post<never, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/report/instances/${id}/publish`,
      );
    },
  }).current;
};
