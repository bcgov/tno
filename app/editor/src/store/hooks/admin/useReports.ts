import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import {
  type IDashboardFilter,
  type IReportFilter,
  type IReportInstanceModel,
  type IReportModel,
  type IReportResultModel,
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
  publishReportInstance: (
    model: IReportInstanceModel,
    resend: boolean,
  ) => Promise<IReportInstanceModel>;
  getDashboard: (filter: IDashboardFilter) => Promise<IReportModel[]>;
  getDashboardReport: (id: number) => Promise<IReportModel>;
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
        const response = await dispatch<IReportModel[]>(
          'find-reports',
          async () => await api.findReports(filter),
        );
        store.storeReports(response.data);
        setInitialized(true);
        return response.data;
      },
      findAllReportsHeadersOnly: async () => {
        const response = await dispatch<IReportModel[]>(
          'find-all-report-headers',
          async () => await api.findAllReportsHeadersOnly(),
        );
        store.storeReports(response.data);
        setInitialized(true);
        return response.data;
      },
      findInstancesForReportId: async (id: number, ownerId: number | undefined = undefined) => {
        const response = await dispatch<IReportInstanceModel[]>(
          'get-report-instances',
          async () => await api.findInstancesForReportId(id, ownerId),
        );
        return response.data;
      },
      getReport: async (id: number) => {
        const response = await dispatch<IReportModel>(
          'get-report',
          async () => await api.getReport(id),
        );
        if (response.status === 200) {
          store.storeReports((reports) =>
            reports.map((ds) => {
              if (ds.id === response.data.id) return response.data;
              return ds;
            }),
          );
          return response.data;
        }
        return await Promise.reject(response);
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'add-report',
          async () => await api.addReport(model),
        );
        store.storeReports((reports) => [...reports, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'update-report',
          async () => await api.updateReport(model),
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
        const response = await dispatch<IReportModel>(
          'delete-report',
          async () => await api.deleteReport(model),
        );
        store.storeReports((reports) => reports.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
      deleteReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>(
          'delete-report-instance',
          async () => await apiInstances.deleteReportInstance(model),
        );
        return response.data;
      },
      sendReport: async (model: IReportModel, to: string) => {
        const response = await dispatch<IReportModel>(
          'send-report',
          async () => await api.sendReport(model, to),
        );
        return response.data;
      },
      publishReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>(
          'publish-report',
          async () => await api.publishReport(model),
        );
        return response.data;
      },
      publishReportInstance: async (model: IReportInstanceModel, resend: boolean) => {
        const response = await dispatch<IReportInstanceModel>(
          'publish-report-instance',
          async () => await apiInstances.publishReportInstance(model, resend),
        );
        return response.data;
      },
      previewReport: async (model: IReportModel) => {
        const response = await dispatch<IReportResultModel>(
          'preview-report',
          async () => await api.previewReport(model),
        );
        return response.data;
      },
      primeReportCache: async (model: IReportModel) => {
        const response = await dispatch<IReportResultModel>(
          'prime-report-cache',
          async () => await api.primeReportCache(model),
        );
        return response.data;
      },
      getDashboard: async (filter: IDashboardFilter) => {
        const response = await dispatch<IReportModel[]>(
          'get-dashboard',
          async () => await api.getDashboard(filter),
        );
        return response.data;
      },
      getDashboardReport: async (id: number) => {
        const response = await dispatch<IReportModel>(
          `get-dashboard-report-${id}`,
          async () => await api.getDashboardReport(id),
        );
        return response.data;
      },
    }),
    [api, apiInstances, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
