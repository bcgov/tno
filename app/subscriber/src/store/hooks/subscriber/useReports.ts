import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import {
  IReportFilter,
  IReportInstanceContentModel,
  IReportInstanceModel,
  IReportModel,
  IReportResultModel,
  IUserModel,
  ReportStatusName,
  sortable,
  useApiSubscriberReports,
} from 'tno-core';

export interface IReportContentMutationModel {
  reportId: number;
  instanceId: number;
  ownerId?: number;
  status: ReportStatusName;
  publishedOn?: string;
  sentOn?: string;
  subject: string;
  body: string;
  response?: any;
  added?: IReportInstanceContentModel[];
  instanceCreated: boolean;
}

const buildSectionOrder = (sections: IReportModel['sections']) => {
  return sections.reduce<Record<string, number>>((acc, section, index) => {
    acc[section.name?.toLowerCase() ?? ''] = index;
    return acc;
  }, {});
};

const sectionKey = (sectionName: string, contentId: number) =>
  `${(sectionName ?? '').toLowerCase()}:${contentId}`;

const mergeInstanceContent = (
  existing: IReportInstanceContentModel[] = [],
  added: IReportInstanceContentModel[] = [],
  sectionOrder: Record<string, number> = {},
): IReportInstanceContentModel[] => {
  if (!added.length) return existing;
  const byKey = new Map<string, IReportInstanceContentModel>();
  existing.forEach((item) => {
    byKey.set(sectionKey(item.sectionName, item.contentId), item);
  });
  added.forEach((item) => {
    byKey.set(sectionKey(item.sectionName, item.contentId), { ...item });
  });
  const merged = Array.from(byKey.values());
  merged.sort((a, b) => {
    const sectionIndexA =
      sectionOrder[a.sectionName?.toLowerCase() ?? ''] ?? Number.MAX_SAFE_INTEGER;
    const sectionIndexB =
      sectionOrder[b.sectionName?.toLowerCase() ?? ''] ?? Number.MAX_SAFE_INTEGER;
    if (sectionIndexA === sectionIndexB) return a.sortOrder - b.sortOrder;
    return sectionIndexA - sectionIndexB;
  });
  return merged;
};

const createInstanceFromMutation = (
  report: IReportModel,
  mutation: IReportContentMutationModel,
  sectionOrder: Record<string, number>,
): IReportInstanceModel => {
  return {
    id: mutation.instanceId,
    reportId: report.id,
    ownerId: mutation.ownerId,
    status: mutation.status,
    subject: mutation.subject,
    body: mutation.body,
    response: mutation.response,
    publishedOn: mutation.publishedOn,
    sentOn: mutation.sentOn,
    content: mergeInstanceContent([], mutation.added ?? [], sectionOrder),
  };
};

const mergeReportWithContentMutation = (
  report: IReportModel,
  mutation: IReportContentMutationModel,
): IReportModel => {
  if (report.id !== mutation.reportId) return report;

  const sectionOrder = buildSectionOrder(report.sections);
  const instances = report.instances ?? [];
  const instanceIndex = instances.findIndex((instance) => instance.id === mutation.instanceId);

  if (instanceIndex === -1) {
    if (!mutation.added?.length) return report;
    const newInstance = createInstanceFromMutation(report, mutation, sectionOrder);
    const nextInstances = mutation.instanceCreated
      ? [newInstance, ...instances]
      : [...instances, newInstance];
    return { ...report, instances: nextInstances };
  }

  const instance = instances[instanceIndex];
  const nextInstances = [...instances];
  nextInstances[instanceIndex] = {
    ...instance,
    ownerId: mutation.ownerId ?? instance.ownerId,
    status: mutation.status ?? instance.status,
    subject: mutation.subject ?? instance.subject,
    body: mutation.body ?? instance.body,
    response: mutation.response ?? instance.response,
    publishedOn: mutation.publishedOn ?? instance.publishedOn,
    sentOn: mutation.sentOn ?? instance.sentOn,
    content: mutation.added?.length
      ? mergeInstanceContent(instance.content ?? [], mutation.added, sectionOrder)
      : instance.content,
  };
  return { ...report, instances: nextInstances };
};

interface IReportController {
  findMyReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  findPublicReports: (filter?: IReportFilter) => Promise<IReportModel[]>;
  getReport: (id: number, includeContent?: boolean) => Promise<IReportModel | undefined>;
  getReportOwner: (id: number) => Promise<IUserModel | undefined>;
  findInstancesForReportId: (id: number, ownerId?: number) => Promise<IReportInstanceModel[]>;
  addReport: (model: IReportModel) => Promise<IReportModel>;
  updateReport: (model: IReportModel, updateInstances?: boolean) => Promise<IReportModel>;
  deleteReport: (model: IReportModel) => Promise<IReportModel>;
  previewReport: (id: number) => Promise<IReportResultModel>;
  generateReport: (id: number, regenerate?: boolean) => Promise<IReportModel>;
  regenerateSection: (id: number, sectionId: number) => Promise<IReportInstanceModel>;
  addContentToReport: (
    id: number,
    content: IReportInstanceContentModel[],
  ) => Promise<IReportContentMutationModel | undefined>;
  getAllContentInMyReports: () => Promise<{ [reportId: number]: number[] }>;
  RequestToSubscribe: (id: number, email: string) => void;
  RequestToUnsubscribe: (id: number, email: string) => void;
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
        const mutation = response.data as unknown as IReportContentMutationModel | undefined;
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
    [api, dispatch, storeMyReports, storeReportContent],
  );

  return [state, controller];
};
