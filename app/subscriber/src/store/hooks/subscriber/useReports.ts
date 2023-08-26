import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IReportFilter, IReportModel, IReportResultModel, useApiSubscriberReports } from 'tno-core';

interface IReportController {
  findReports: (filter: IReportFilter) => Promise<IReportModel[]>;
  findMyReports: () => Promise<IReportModel[]>;
  findAllReports: () => Promise<IReportModel[]>;
  getReport: (id: number) => Promise<IReportModel | undefined>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (reportId: number) => Promise<IReportResultModel>;
}

export const useReports = (): [IReportController] => {
  const api = useApiSubscriberReports();
  const dispatch = useAjaxWrapper();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      getPublicReports: async () => {
        const response = await dispatch<IReportModel[]>('find-all-reports', () =>
          api.getPublicReports(),
        );
        return response.data;
      },
      findReports: async (filter: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-all-reports', () =>
          api.findReports(filter),
        );
        return response.data;
      },
      findMyReports: async () => {
        const response = await dispatch<IReportModel[]>('find-my-folders', () =>
          api.findMyReports(),
        );
        return response.data;
      },
      getReport: async (id: number) => {
        const response = await dispatch<IReportModel | undefined>('get-report', () =>
          api.getReport(id),
        );
        return response.data;
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('add-report', () => api.addReport(model));
        await lookup.getLookups();
        return response.data;
      },
      updateReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport(model),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('delete-report', () =>
          api.deleteReport(model),
        );
        await lookup.getLookups();
        return response.data;
      },
      previewReport: async (reportId: number) => {
        const response = await dispatch<IReportResultModel>('preview-report', () =>
          api.previewReport(reportId),
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup],
  );

  return [controller];
};
