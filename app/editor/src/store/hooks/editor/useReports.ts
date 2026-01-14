import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import {
  type IReportFilter,
  type IReportModel,
  type IReportResultModel,
  useApiEditorReports,
} from 'tno-core';

interface IReportController {
  findReports: (filter: IReportFilter) => Promise<IReportModel[]>;
  getReport: (id: number) => Promise<IReportModel | undefined>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (reportId: number) => Promise<IReportResultModel>;
  publishReport: (reportId: number) => Promise<never>;
}

export const useReports = (): [IReportController] => {
  const api = useApiEditorReports();
  const dispatch = useAjaxWrapper();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findReports: async (filter: IReportFilter) => {
        const response = await dispatch<IReportModel[]>(
          'find-all-reports',
          async () => await api.findReports(filter),
        );
        return response.data;
      },
      getReport: async (id: number) => {
        const response = await dispatch<IReportModel | undefined>(
          'get-report',
          async () => await api.getReport(id),
        );
        return response.data;
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'add-report',
          async () => await api.addReport(model),
        );
        await lookup.getLookups();
        return response.data;
      },
      updateReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'update-report',
          async () => await api.updateReport(model),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'delete-report',
          async () => await api.deleteReport(model),
        );
        await lookup.getLookups();
        return response.data;
      },
      previewReport: async (reportId: number) => {
        const response = await dispatch<IReportResultModel>(
          'preview-report',
          async () => await api.previewReport(reportId),
        );
        return response.data;
      },
      publishReport: async (reportId: number) => {
        const response = await dispatch<never>(
          'publish-report',
          async () => await api.publishReport(reportId),
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup],
  );

  return [controller];
};
