import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  IReportFilter,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  sortable,
  useApiSubscriberReports,
} from 'tno-core';

interface IReportController {
  findMyReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  findPublicReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  getReport: (id: number) => Promise<IReportModel | undefined>;
  findInstancesForReportId: (id: number, ownerId?: number) => Promise<IReportInstanceModel[]>;
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
  const [, { storeMyReports }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findMyReports: async (filter?: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-my-reports', async () => {
          return api.findMyReports(filter);
        });
        storeMyReports(response.data);
        return response.data;
      },
      findPublicReports: async (filter?: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-public-reports', () =>
          api.findPublicReports(filter),
        );
        return response.data;
      },
      getReport: async (id: number) => {
        const response = await dispatch<IReportModel | undefined>('get-report', () =>
          api.getReport(id),
        );
        if (response.data) {
          storeMyReports((reports) => {
            if (reports.some((r) => r.id === response.data?.id))
              return reports.map((r) => (r.id === response.data?.id ? response.data : r));
            return [response.data!, ...reports];
          });
        }
        return response.data;
      },
      findInstancesForReportId: async (id: number, ownerId: number | undefined = undefined) => {
        const response = await dispatch<IReportInstanceModel[]>('get-report-instances', () =>
          api.findInstancesForReportId(id, ownerId),
        );
        storeMyReports((reports) =>
          reports.map((r) => (r.id === id ? { ...r, instances: response.data } : r)),
        );
        return response.data;
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('add-report', () => api.addReport(model));
        // Update store with report data.
        storeMyReports((reports) => {
          return [response.data!, ...reports].sort(sortable);
        });
        return response.data;
      },
      updateReport: async (model: IReportModel, updateInstances: boolean | undefined) => {
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport(model, updateInstances),
        );
        storeMyReports((reports) => {
          if (reports.some((r) => r.id === response.data?.id))
            return reports.map((r) => (r.id === model.id ? response.data : r));
          return [response.data!, ...reports];
        });
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('delete-report', () =>
          api.deleteReport(model),
        );
        storeMyReports((reports) => reports.filter((r) => r.id !== model.id));
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
        storeMyReports((reports) => reports.map((r) => (r.id === id ? response.data : r)));
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
    [api, dispatch, storeMyReports],
  );

  return [controller];
};
