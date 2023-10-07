import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IReportFilter, IReportModel, IReportResultModel, useApiSubscriberReports } from 'tno-core';

interface IReportController {
  findReports: (filter: IReportFilter) => Promise<IReportModel[]>;
  findMyReports: () => Promise<IReportModel[]>;
  getPublicReports: () => Promise<IReportModel[]>;
  getReport: (id: number) => Promise<IReportModel | undefined>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel, updateInstances?: boolean) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (id: number) => Promise<IReportResultModel>;
  generateReport: (id: number, regenerate?: boolean) => Promise<IReportModel>;
  sendReport: (id: number, to: string) => Promise<IReportModel>;
  publishReport: (id: number) => Promise<IReportModel>;
}

export const useReports = (): [IReportController] => {
  const api = useApiSubscriberReports();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      findReports: async (filter: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-reports', () =>
          api.findReports(filter),
        );
        return response.data;
      },
      findMyReports: async () => {
        const response = await dispatch<IReportModel[]>('find-my-reports', async () => {
          return api.findMyReports();
        });
        return response.data;
      },
      getPublicReports: async () => {
        const response = await dispatch<IReportModel[]>('get-public-reports', () =>
          api.getPublicReports(),
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
        return response.data;
      },
      updateReport: async (model: IReportModel, updateInstances: boolean | undefined) => {
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport(model, updateInstances),
        );
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('delete-report', () =>
          api.deleteReport(model),
        );
        return response.data;
      },
      previewReport: async (id: number) => {
        const response = await dispatch<IReportResultModel>('preview-report', () =>
          api.previewReport(id),
        );
        return response.data;
      },
      generateReport: async (id: number, regenerate: boolean | undefined = false) => {
        const response = await dispatch<IReportModel>('generate-report', () =>
          api.generateReport(id, regenerate),
        );
        return response.data;
      },
      sendReport: async (id: number, to: string) => {
        const response = await dispatch<IReportModel>('send-report', () => api.sendReport(id, to));
        return response.data;
      },
      publishReport: async (id: number) => {
        const response = await dispatch<IReportModel>('publish-report', () =>
          api.publishReport(id),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
