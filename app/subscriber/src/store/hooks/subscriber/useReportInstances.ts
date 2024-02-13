import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  IReportInstanceModel,
  IReportResultModel,
  useApiSubscriberReportInstances,
} from 'tno-core';

interface IReportInstanceController {
  getReportInstance: (
    id: number,
    includeContent?: boolean,
  ) => Promise<IReportInstanceModel | undefined>;
  addReportInstance: (model: IReportInstanceModel) => Promise<IReportInstanceModel>;
  updateReportInstance: (model: IReportInstanceModel) => Promise<IReportInstanceModel>;
  deleteReportInstance: (model: IReportInstanceModel) => Promise<IReportInstanceModel>;
  viewReportInstance: (id: number, regenerate?: boolean) => Promise<IReportResultModel>;
  sendReportInstance: (id: number, to: string) => Promise<IReportInstanceModel>;
  publishReportInstance: (id: number) => Promise<IReportInstanceModel>;
  exportReport: (id: number, reportName: string) => Promise<IReportInstanceModel>;
}

export const useReportInstances = (): [IReportInstanceController] => {
  const api = useApiSubscriberReportInstances();
  const dispatch = useAjaxWrapper();
  const [, { storeReportContent }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      getReportInstance: async (id: number, includeContent: boolean = false) => {
        const response = await dispatch<IReportInstanceModel | undefined>(
          'get-report-instance',
          () => api.getReportInstance(id, includeContent),
        );
        if (response.status === 200 && !!response.data) {
          const instance = response.data;
          storeReportContent((reports) => {
            const result = { ...reports };
            result[instance.reportId] = instance.content.length
              ? instance.content.map((c) => c.contentId)
              : reports[instance.reportId] ?? [];
            return result;
          });
        }
        return response.data;
      },
      addReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>('add-report-instance', () =>
          api.addReportInstance(model),
        );
        if (response.status === 201 && !!response.data) {
          const instance = response.data;
          storeReportContent((reports) => {
            const result = { ...reports };
            result[instance.reportId] = instance.content.length
              ? instance.content.map((c) => c.contentId)
              : reports[instance.reportId] ?? [];
            return result;
          });
        }
        return response.data;
      },
      updateReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>('update-report-instance', () =>
          api.updateReportInstance(model),
        );
        if (response.status === 200 && !!response.data) {
          const instance = response.data;
          storeReportContent((reports) => {
            const result = { ...reports };
            result[instance.reportId] = instance.content.length
              ? instance.content.map((c) => c.contentId)
              : reports[instance.reportId] ?? [];
            return result;
          });
        }
        return response.data;
      },
      deleteReportInstance: async (model: IReportInstanceModel) => {
        const response = await dispatch<IReportInstanceModel>('delete-report-instance', () =>
          api.deleteReportInstance(model),
        );
        if (response.status === 200 && !!response.data) {
          const instance = response.data;
          storeReportContent((reports) => {
            const result = { ...reports };
            delete result[instance.reportId];
            return result;
          });
        }
        return response.data;
      },
      viewReportInstance: async (id: number, regenerate: boolean = false) => {
        const response = await dispatch<IReportResultModel>(
          'view-report-instance',
          () => api.viewReportInstance(id, regenerate),
          'view-report',
          true,
        );
        return response.data;
      },
      sendReportInstance: async (id: number, to: string) => {
        const response = await dispatch<IReportInstanceModel>('send-report-instance', () =>
          api.sendReportInstance(id, to),
        );
        return response.data;
      },
      publishReportInstance: async (id: number) => {
        const response = await dispatch<IReportInstanceModel>('publish-report-instance', () =>
          api.publishReportInstance(id),
        );
        return response.data;
      },
      exportReport: async (id: number, reportName: string) => {
        const response = await dispatch<IReportInstanceModel>('export-report', () =>
          api.exportReport(id, reportName),
        );
        return response.data;
      },
    }),
    [api, dispatch, storeReportContent],
  );

  return [controller];
};
