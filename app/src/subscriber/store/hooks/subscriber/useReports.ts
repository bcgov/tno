import { AxiosError } from 'axios';
import React from 'react';
import { useAjaxWrapper } from '@/subscriber/store/hooks';
import { IProfileState, useProfileStore } from '@/subscriber/store/slices';
import {
  IReportFilter,
  IReportInstanceContentModel,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  IUserModel,
  IUserReportModel,
  sortable,
  useApi,
  useApiSubscriberReports,
} from 'tno-core';

import { IChartDataPreviewRequestModel } from './interfaces/IChartDataPreviewRequestModel';
import { IReportContentMutationModel } from './interfaces/IReportContentMutationModel';
import { mergeReportWithContentMutation } from './utils/reportContentMutationHelpers';

interface IReportController {
  findMyReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  findPublicReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  getReport: (id: number, includeContent?: boolean) => Promise<IReportModel | undefined>;
  getReportOwner: (id: number) => Promise<IUserModel | undefined>;
  findInstancesForReportId: (id: number, ownerId?: number) => Promise<IReportInstanceModel[]>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel, updateInstances?: boolean) => Promise<IReportModel>;
  updateReportSubscribers: (id: number, subscribers: IUserReportModel[]) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (id: number) => Promise<IReportResultModel>;
  generateReport: (id: number, regenerate?: boolean) => Promise<IReportModel>;
  regenerateSection: (id: number, sectionId: number) => Promise<IReportInstanceModel>;
  /** Fetch chart data for unsaved chart values. Resolves with undefined when there are no stories to take data from yet. */
  getSectionChartData: (
    id: number,
    sectionId: number,
    chartTemplateId: number,
    model: IChartDataPreviewRequestModel,
  ) => Promise<any | undefined>;
  addContentToReport: (
    id: number,
    content: IReportInstanceContentModel[],
  ) => Promise<IReportContentMutationModel | undefined>;
  getAllContentInMyReports: () => Promise<{ [reportId: number]: number[] }>;
  RequestToSubscribe: (id: number, email: string) => void;
  RequestToUnsubscribe: (id: number, email: string) => void;
}

/**
 * The version of the list of reports in the store, as the API described it when it sent them.
 * The list lives as long as the page does, so its version does too.
 */
let myReportsVersion: string | undefined;

export const useReports = (): [IProfileState, IReportController] => {
  const api = useApiSubscriberReports();
  const http = React.useRef(useApi());
  const dispatch = useAjaxWrapper();
  const [state, { storeMyReports, storeReportContent }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findMyReports: async (filter?: IReportFilter) => {
        // Only the whole list is kept, so only it can be checked. An empty store has nothing to keep.
        const version = !filter && state.myReports.length ? myReportsVersion : undefined;
        try {
          const response = await dispatch<IReportModel[]>(
            'find-my-reports',
            async () => {
              return version
                ? http.current.get<IReportModel[]>('/subscriber/reports/my-reports', {
                    headers: { 'If-None-Match': version },
                  })
                : api.findMyReports(filter);
            },
            undefined,
            // Asking whether the list is stale is not something to make the user wait for.
            !!version,
          );
          if (response.status === 200) {
            // The list comes without report instances. Those already loaded stay with their report.
            storeMyReports((reports) =>
              response.data.map((report) => {
                const instances = reports.find((r) => r.id === report.id)?.instances;
                return instances?.length && !report.instances?.length
                  ? { ...report, instances }
                  : report;
              }),
            );
            if (!filter) myReportsVersion = response.headers['etag'];
          }
          return response.data;
        } catch (error) {
          // The reports have not changed since they were stored.
          if ((error as AxiosError).response?.status === 304) return state.myReports;
          throw error;
        }
      },
      findPublicReports: async (filter?: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-public-reports', () =>
          api.findPublicReports(filter),
        );
        return response.data;
      },
      getReport: async (id: number, includeContent?: boolean) => {
        const response = await dispatch<IReportModel | undefined>(
          `get-report-${id}`,
          () => api.getReport(id, includeContent),
          'get-report',
        );
        if (response.status === 200 && response.data) {
          storeMyReports((reports) => {
            if (!response.data) return reports;
            var contains = false;
            const results = reports.map((r) => {
              if (r.id === id) contains = true;
              return r.id === id ? response.data! : r;
            });

            if (contains) return results;
            return [response.data!, ...reports];
          });
          storeReportContent((reports) => {
            const result = { ...reports };
            result[id] = response.data?.instances.length
              ? response.data.instances[0].content.map((c) => c.contentId)
              : reports[id] ?? [];
            return result;
          });
        }
        return response.data;
      },
      getReportOwner: async (id: number) => {
        const response = await dispatch<IUserModel | undefined>('get-report-owner', () =>
          api.getReportOwner(id),
        );
        return response.data;
      },
      findInstancesForReportId: async (
        id: number,
        ownerId?: number,
        page?: number,
        quantity?: number,
      ) => {
        const response = await dispatch<IReportInstanceModel[]>('get-report-instances', () =>
          api.findInstancesForReportId(id, ownerId, page, quantity),
        );
        if (response.status === 200) {
          storeMyReports((reports) =>
            reports.map((r) => (r.id === id ? { ...r, instances: response.data } : r)),
          );
          storeReportContent((reports) => {
            const result = { ...reports };
            result[id] = response.data.length
              ? response.data[0].content.map((c) => c.contentId)
              : reports[id] ?? [];
            return result;
          });
        }
        return response.data;
      },
      addReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('add-report', () => api.addReport(model));
        // Update store with report data.
        if (response.status === 201) {
          storeMyReports((reports) => {
            if (!response.data) return reports;
            return [response.data!, ...reports].sort(sortable);
          });
          storeReportContent((reports) => {
            const result = { ...reports };
            result[model.id] = response.data.instances.length
              ? response.data.instances[0].content.map((c) => c.contentId)
              : reports[model.id] ?? [];
            return result;
          });
        }
        return response.data;
      },
      updateReport: async (model: IReportModel, updateInstances: boolean | undefined) => {
        // Subscribers are managed through 'updateReportSubscribers'.  The API ignores them here,
        // which stops a stale form snapshot from unsubscribing users who were added elsewhere.
        // The API only saves the first instance's stories when 'updateInstances' is set, so they are
        // left out otherwise; a report can hold tens of thousands of them.
        const instances = updateInstances
          ? model.instances
          : model.instances.map((instance) => ({ ...instance, content: [] }));
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport({ ...model, instances, subscribers: [] }, updateInstances),
        );
        if (response.status === 200) {
          storeMyReports((reports) => {
            if (!response.data) return reports;
            var contains = false;
            const results = reports.map((r) => {
              if (r.id === model.id) contains = true;
              return r.id === model.id ? response.data! : r;
            });

            if (contains) return results;
            return [response.data!, ...reports];
          });
          storeReportContent((reports) => {
            const result = { ...reports };
            result[model.id] = response.data.instances.length
              ? response.data.instances[0].content.map((c) => c.contentId)
              : reports[model.id] ?? [];
            return result;
          });
        }
        return response.data;
      },
      updateReportSubscribers: async (id: number, subscribers: IUserReportModel[]) => {
        const response = await dispatch<IReportModel>('update-report-subscribers', () =>
          api.updateReportSubscribers(id, subscribers),
        );
        if (response.status === 200 && response.data) {
          storeMyReports((reports) =>
            reports.map((r) =>
              r.id === id ? { ...r, subscribers: response.data.subscribers } : r,
            ),
          );
        }
        return response.data;
      },
      deleteReport: async (model: IReportModel) => {
        const response = await dispatch<IReportModel>('delete-report', () =>
          api.deleteReport(model),
        );
        if (response.status === 200) {
          storeMyReports((reports) => reports.filter((r) => r.id !== model.id));
          storeReportContent((reports) => {
            const result = { ...reports };
            delete result[model.id];
            return result;
          });
        }
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
        if (response.status === 200) {
          storeMyReports((reports) => reports.map((r) => (r.id === id ? response.data : r)));
          storeReportContent((reports) => {
            const result = { ...reports };
            result[id] = response.data.instances.length
              ? response.data.instances[0].content.map((c) => c.contentId)
              : reports[id] ?? [];
            return result;
          });
        }
        return response.data;
      },
      getSectionChartData: async (
        id: number,
        sectionId: number,
        chartTemplateId: number,
        model: IChartDataPreviewRequestModel,
      ) => {
        // Called while typing, so a failure must not raise the global error toast.
        const response = await http.current.post(
          `/subscriber/reports/${id}/sections/${sectionId}/charts/${chartTemplateId}/data`,
          model,
        );
        return response.status === 200 ? response.data : undefined;
      },
      regenerateSection: async (id: number, sectionId: number) => {
        const response = await dispatch<IReportInstanceModel>('generate-report-section', () =>
          api.regenerateReportSection(id, sectionId),
        );
        if (response.status === 200) {
          storeMyReports((reports) =>
            reports.map((r) =>
              r.id === response.data.reportId
                ? {
                    ...r,
                    instances: r.instances.map((instance) =>
                      instance.id === response.data.id ? response.data : instance,
                    ),
                  }
                : r,
            ),
          );
          storeReportContent((reports) => {
            const result = { ...reports };
            result[id] = response.data.content.map((c) => c.contentId);
            return result;
          });
        }
        return response.data;
      },
      addContentToReport: async (id: number, content: IReportInstanceContentModel[]) => {
        const response = await dispatch<IReportContentMutationModel>('add-content-to-report', () =>
          api.addContentToReport(id, content),
        );
        const mutation = response.data;
        if (response.status === 200 && mutation) {
          const addedIds =
            mutation.added?.map(
              (contentItem: IReportInstanceContentModel) => contentItem.contentId,
            ) ?? [];
          if (addedIds.length) {
            storeReportContent((reports) => {
              const existing = reports[id] ?? [];
              const merged = Array.from(new Set([...existing, ...addedIds]));
              return { ...reports, [id]: merged };
            });
          }
          storeMyReports((reports) =>
            reports.map((report) => mergeReportWithContentMutation(report, mutation)),
          );
        }
        return mutation;
      },
      getAllContentInMyReports: async () => {
        const response = await dispatch<{ [reportId: number]: number[] }>(
          'get-all-report-content',
          () => api.getAllContentInMyReports(),
        );
        return response.data;
      },
      RequestToSubscribe: async (id: number, applicantEmail: string) => {
        const response = await dispatch<{ [reportId: number]: number[] }>('request-subscribe', () =>
          api.RequestToSubscribe(id, applicantEmail),
        );
        return response.data;
      },
      RequestToUnsubscribe: async (id: number, applicantEmail: string) => {
        const response = await dispatch<{ [reportId: number]: number[] }>(
          'request-unsubscribe',
          () => api.RequestToUnsubscribe(id, applicantEmail),
        );
        return response.data;
      },
    }),
    [api, dispatch, state.myReports, storeMyReports, storeReportContent],
  );

  return [state, controller];
};
