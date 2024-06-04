import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import {
  IReportFilter,
  IReportInstanceContentModel,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  sortable,
  useApiSubscriberReports,
} from 'tno-core';

interface IReportController {
  findMyReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  findPublicReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  getReport: (id: number, includeContent?: boolean) => Promise<IReportModel | undefined>;
  findInstancesForReportId: (id: number, ownerId?: number) => Promise<IReportInstanceModel[]>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel, updateInstances?: boolean) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (id: number) => Promise<IReportResultModel>;
  generateReport: (id: number, regenerate?: boolean) => Promise<IReportModel>;
  regenerateSection: (id: number, sectionId: number) => Promise<IReportInstanceModel>;
  addContentToReport: (id: number, content: IReportInstanceContentModel[]) => Promise<IReportModel>;
  getAllContentInMyReports: () => Promise<{ [reportId: number]: number[] }>;
  findInstanceForReportIdAndDate: (id: number, date: Date) => Promise<IReportInstanceModel>;
}

export const useReports = (): [IProfileState, IReportController] => {
  const api = useApiSubscriberReports();
  const dispatch = useAjaxWrapper();
  const [state, { storeMyReports, storeReportContent }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findMyReports: async (filter?: IReportFilter) => {
        const response = await dispatch<IReportModel[]>('find-my-reports', async () => {
          return api.findMyReports(filter);
        });
        if (response.status === 200) {
          storeMyReports(response.data);
        }
        return response.data;
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
      findInstanceForReportIdAndDate: async (id: number, date: Date) => {
        const response = await dispatch<IReportInstanceModel[]>('get-report-instance-by-date', () =>
          api.findInstanceForReportIdAndDate(id, date),
        );
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
        const response = await dispatch<IReportModel>('update-report', () =>
          api.updateReport(model, updateInstances),
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
        const response = await dispatch<IReportModel>('add-content-to-report', () =>
          api.addContentToReport(id, content),
        );
        if (response.status === 200) {
          storeMyReports((reports) => {
            if (!response.data) return reports;
            var contains = false;
            const results = reports.map((r) => {
              // If the report exists then we don't need to add it.
              if (r.id === id) contains = true;
              return r.id === id ? response.data! : r;
            });

            if (contains) return results;
            return [response.data!, ...reports];
          });
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
      getAllContentInMyReports: async () => {
        const response = await dispatch<{ [reportId: number]: number[] }>(
          'get-all-report-content',
          () => api.getAllContentInMyReports(),
        );
        return response.data;
      },
    }),
    [api, dispatch, storeMyReports, storeReportContent],
  );

  return [state, controller];
};
