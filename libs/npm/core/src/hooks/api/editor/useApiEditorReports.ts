import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { IReportFilter, IReportModel, IReportResultModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiEditorReports = (
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
        `/editor/reports?${toQueryString(query)}`,
      );
    },
    getReport: (id: number) => {
      return api.get<never, AxiosResponse<IReportModel | undefined>, any>(`/editor/reports/${id}`);
    },
    addReport: (report: IReportModel) => {
      return api.post<IReportModel, AxiosResponse<IReportModel>, any>('/editor/reports', report);
    },
    updateReport: (report: IReportModel) => {
      return api.put<IReportModel, AxiosResponse<IReportModel>, any>(
        `/editor/reports/${report.id}`,
        report,
      );
    },
    deleteReport: (report: IReportModel) => {
      return api.delete<IReportModel, AxiosResponse<IReportModel>, any>(
        `/editor/reports/${report.id}`,
        { data: report },
      );
    },
    previewReport: (reportId: number) => {
      return api.post<never, AxiosResponse<IReportResultModel>, any>(
        `/editor/reports/${reportId}/preview`,
      );
    },
    publishReport: (reportId: number) => {
      return api.post<never, AxiosResponse<never>, any>(`/editor/reports/${reportId}/publish`);
    },
  }).current;
};
