import { IReportInstanceContentModel, IReportInstanceModel, IReportModel } from 'tno-core';

import { IReportContentMutationModel } from '../interfaces/IReportContentMutationModel';

export const buildSectionOrder = (sections: IReportModel['sections']) => {
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

export const mergeReportWithContentMutation = (
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
