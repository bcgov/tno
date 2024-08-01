import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import {
  IReportDashboard,
  IReportFilter,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  useApiAdminReportInstances,
  useApiAdminReports,
} from 'tno-core';

interface IReportController {
  findReports: (filter: IReportFilter) => Promise<IReportModel[]>;
  findAllReportsHeadersOnly: () => Promise<IReportModel[]>;
  findInstancesForReportId: (id: number, ownerId?: number) => Promise<IReportInstanceModel[]>;
  getReport: (id: number) => Promise<IReportModel>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  sendReport: (model: IReportModel, to: string) => Promise<IReportModel>;
  publishReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (model: IReportModel) => Promise<IReportResultModel>;
  primeReportCache: (model: IReportModel) => Promise<IReportResultModel>;
  deleteReportInstance: (model: IReportInstanceModel) => Promise<IReportInstanceModel>;
  publishReportInstance: (model: IReportInstanceModel) => Promise<IReportInstanceModel>;
  getDashboard: () => Promise<IReportDashboard>;
}

export const useReports = (): [IAdminState & { initialized: boolean }, IReportController] => {
  const api = useApiAdminReports();
  const apiInstances = useApiAdminReportInstances();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findReports: async (filter: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-reports', () =>
          api.findReports(filter),
        );
        store.storeReports(response.data);
        setInitialized(true);
        return response.data;
      },
      findAllReportsHeadersOnly: async () => {
        const response = await dispatch<IReportModel[]>('find-all-report-headers', () =>
          api.findAllReportsHeadersOnly(),
        );
        store.storeReports(response.data);
        setInitialized(true);
        return response.data;
      },
      findInstancesForReportId: async (id: number, ownerId: number | undefined = undefined) => {
        const response = await dispatch<IReportInstanceModel[]>('get-report-instances', () =>
          api.findInstancesForReportId(id, ownerId),
        );
        return response.data;
      },
      getReport: async (id: number) => {
        const response = await dispatch<IReportModel>('get-report', () => api.getReport(id));
        if (response.status === 200) {
          store.storeReports((reports) =>
            reports.map((ds) => {
              if (ds.id === response.data.id) return response.data;
              return ds;
            }),
          );
          return response.data;
        }
        return Promise.reject(response);
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('add-report', () => api.addReport(model));
        store.storeReports((reports) => [...reports, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport(model),
        );
        store.storeReports((reports) =>
          reports.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('delete-report', () =>
          api.deleteReport(model),
        );
        store.storeReports((reports) => reports.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
      deleteReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>('delete-report-instance', () =>
          apiInstances.deleteReportInstance(model),
        );
        return response.data;
      },
      sendReport: async (model: IReportModel, to: string) => {
        const response = await dispatch<IReportModel>('send-report', () =>
          api.sendReport(model, to),
        );
        return response.data;
      },
      publishReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('publish-report', () =>
          api.publishReport(model),
        );
        return response.data;
      },
      publishReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>('publish-report-instance', () =>
          apiInstances.publishReportInstance(model),
        );
        return response.data;
      },
      previewReport: async (model: IReportModel) => {
        const response = await dispatch<IReportResultModel>('preview-report', () =>
          api.previewReport(model),
        );
        return response.data;
      },
      primeReportCache: async (model: IReportModel) => {
        const response = await dispatch<IReportResultModel>('prime-report-cache', () =>
          api.primeReportCache(model),
        );
        return response.data;
      },
      getDashboard: async () => {
        const response = await dispatch<IReportDashboard>('get-dashboard', () =>
          api.getDashboard(),
        );
        return response.data;
      },
    }),
    [api, apiInstances, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
