import { IReportInstanceContentModel } from 'tno-core';

import { IReportForm } from '../interfaces';

const DUPLICATE_WINDOW_MS = 3 * 24 * 60 * 60 * 1000;

const normalizeHeadline = (headline?: string | null) => headline?.trim().toUpperCase() ?? '';

const parseDate = (value?: string | Date | null) => {
  if (!value) return undefined;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? undefined : date;
};

type ReportContentItem = IReportInstanceContentModel & {
  originalIndex?: number;
  selected?: boolean;
};

const getComparableDate = (content?: ReportContentItem['content']) =>
  parseDate(content?.publishedOn) ??
  parseDate(content?.postedOn) ??
  parseDate(content?.updatedOn) ??
  parseDate(content?.createdOn) ??
  new Date(0);

const getUpdatedDate = (content?: ReportContentItem['content']) =>
  parseDate(content?.updatedOn) ?? new Date(0);

const getCreatedDate = (content?: ReportContentItem['content']) =>
  parseDate(content?.createdOn) ?? new Date(0);

const dedupeContent = (
  items: ReportContentItem[],
  predicate: (item: ReportContentItem) => boolean,
  threshold: Date,
) => {
  if (!items.length) return items;

  const indicesToDrop = new Set<number>();
  const groups = new Map<string, number[]>();

  items.forEach((item, index) => {
    if (!predicate(item)) return;
    const title = normalizeHeadline(item.content?.headline);
    if (!title) return;
    const date = getComparableDate(item.content);
    if (date < threshold) return;
    const existing = groups.get(title);
    if (existing) existing.push(index);
    else groups.set(title, [index]);
  });

  const compareItems = (a: ReportContentItem, b: ReportContentItem) => {
    const dateA = getComparableDate(a.content);
    const dateB = getComparableDate(b.content);
    if (dateA.getTime() !== dateB.getTime()) return dateB.getTime() - dateA.getTime();

    const updatedA = getUpdatedDate(a.content);
    const updatedB = getUpdatedDate(b.content);
    if (updatedA.getTime() !== updatedB.getTime()) return updatedB.getTime() - updatedA.getTime();

    const createdA = getCreatedDate(a.content);
    const createdB = getCreatedDate(b.content);
    if (createdA.getTime() !== createdB.getTime()) return createdB.getTime() - createdA.getTime();

    if ((a.contentId ?? 0) !== (b.contentId ?? 0)) return (b.contentId ?? 0) - (a.contentId ?? 0);

    return (b.sortOrder ?? 0) - (a.sortOrder ?? 0);
  };

  groups.forEach((indices) => {
    if (indices.length <= 1) return;
    const ordered = indices.slice().sort((a, b) => compareItems(items[a], items[b]));
    const [, ...others] = ordered;
    others.forEach((idx) => indicesToDrop.add(idx));
  });

  if (!indicesToDrop.size) return items;

  return items.filter((_, index) => !indicesToDrop.has(index));
};

const reassignSortOrders = <T extends ReportContentItem>(items: T[]): T[] => {
  const orderCounters = new Map<string, number>();
  return items.map((item) => {
    const current = orderCounters.get(item.sectionName) ?? 0;
    orderCounters.set(item.sectionName, current + 1);
    return { ...item, sortOrder: current };
  });
};

/**
 * Ensures report-level and section-level settings include explicit boolean values for
 * removeDuplicateTitles3Days and applies dedupe logic before persisting.
 */
export const sanitizeReport = (report: IReportForm): IReportForm => {
  const sanitizedSections = report.sections.map((section) => ({
    ...section,
    settings: {
      ...section.settings,
      removeDuplicateTitles3Days: !!section.settings.removeDuplicateTitles3Days,
    },
  }));

  const sanitizedSettings = {
    ...report.settings,
    content: {
      ...report.settings.content,
      removeDuplicateTitles3Days: !!report.settings.content.removeDuplicateTitles3Days,
    },
  };

  const threshold = new Date(Date.now() - DUPLICATE_WINDOW_MS);

  const sanitizedInstances = report.instances.map((instance) => {
    const originalContent = instance.content ?? [];
    let content: ReportContentItem[] = [...originalContent];

    sanitizedSections
      .filter((section) => section.settings.removeDuplicateTitles3Days)
      .forEach((section) => {
        content = dedupeContent(content, (item) => item.sectionName === section.name, threshold);
      });

    if (sanitizedSettings.content.removeDuplicateTitles3Days)
      content = dedupeContent(content, () => true, threshold);

    content = reassignSortOrders(content);

    return {
      ...instance,
      content,
    };
  });

  return {
    ...report,
    settings: sanitizedSettings,
    sections: sanitizedSections,
    instances: sanitizedInstances,
  };
};
