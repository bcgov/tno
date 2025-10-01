import { AxiosResponse } from 'axios';
import React from 'react';

import { toQueryString } from '../../../utils';
import {
  defaultEnvelope,
  ILifecycleToasts,
  IReportContentMutationModel,
  IReportFilter,
  IReportInstanceContentModel,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  IUserModel,
  useApi,
} from '../..';

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
    findMyReports: (filter?: IReportFilter) => {
      const query = { ...filter };
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports/my-reports?${toQueryString(query)}`,
      );
    },
    findPublicReports: (filter?: IReportFilter) => {
      const query = { ...filter };
      return api.get<IReportModel[], AxiosResponse<IReportModel[]>, any>(
        `/subscriber/reports/public?${toQueryString(query)}`,
      );
    },
    getReport: (id: number, includeContent?: boolean) => {
      return api.get<never, AxiosResponse<IReportModel | undefined>, any>(
        `/subscriber/reports/${id}${includeContent ? `?includeContent=true` : ''}`,
      );
    },
    getReportOwner: (id: number) => {
      return api.get<never, AxiosResponse<IUserModel | undefined>, any>(
        `/subscriber/reports/${id}/owner`,
      );
    },
    findInstancesForReportId: (
      id: number,
      ownerId: number | undefined = undefined,
      page?: number,
      quantity?: number,
    ) => {
      const query = { ownerId, page, qty: quantity };
      return api.get<never, AxiosResponse<IReportInstanceModel[]>, any>(
        `/subscriber/reports/${id}/instances${toQueryString(query)}`,
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
      return api.post<IReportInstanceContentModel[], AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${reportId}/generate${
          regenerate !== undefined ? `?regenerate=${regenerate}` : ''
        }`,
      );
    },
    regenerateReportSection: (reportId: number, sectionId: number) => {
      return api.post<never, AxiosResponse<IReportInstanceModel>, any>(
        `/subscriber/reports/${reportId}/sections/${sectionId}/regenerate`,
      );
    },
    addContentToReport: (reportId: number, content: IReportInstanceContentModel[]) => {
      return api.post<IReportInstanceContentModel[], AxiosResponse<IReportContentMutationModel>, any>(
        `/subscriber/reports/${reportId}/content/fast`,
        content,
      );
    },
    getAllContentInMyReports: () => {
      return api.get<never, AxiosResponse<{ [reportId: number]: number[] }>, any>(
        `/subscriber/reports/all-content`,
      );
    },
    RequestToSubscribe: (reportId: number, applicantEmail: string) => {
      return api.post<never, AxiosResponse<any>, any>(
        `/subscriber/reports/${reportId}/subscription?applicantEmail=${applicantEmail}`,
      );
    },
    RequestToUnsubscribe: (reportId: number, applicantEmail: string) => {
      return api.post<never, AxiosResponse<any>, any>(
        `/subscriber/reports/${reportId}/unsubscription?applicantEmail=${applicantEmail}`,
      );
    },
  }).current;
};
