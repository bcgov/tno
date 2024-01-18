import { ReportSectionTypeName } from 'tno-core';

import { IReportSectionForm } from '../interfaces';

export const defaultReportSection = (
  type: ReportSectionTypeName,
  sortOrder: number,
  showHeadlines: boolean | undefined = undefined,
  showFullStory: boolean | undefined = undefined,
  hideEmpty: boolean = false,
): IReportSectionForm => ({
  id: 0,
  name: crypto.randomUUID(),
  sectionType: type,
  description: '',
  sortOrder: sortOrder,
  isEnabled: true,
  reportId: 0,
  settings: {
    label:
      type === ReportSectionTypeName.TableOfContents
        ? 'Table of Contents'
        : type === ReportSectionTypeName.Text
        ? 'Executive Summary'
        : '',
    useAllContent: type === ReportSectionTypeName.MediaAnalytics,
    showHeadlines: showHeadlines ?? type === ReportSectionTypeName.TableOfContents,
    showFullStory: showFullStory ?? type === ReportSectionTypeName.Content,
    showImage: false,
    direction: 'row',
    removeDuplicates: false,
    hideEmpty:
      type === ReportSectionTypeName.TableOfContents ||
      type === ReportSectionTypeName.MediaAnalytics ||
      type === ReportSectionTypeName.Gallery
        ? false
        : hideEmpty,
    groupBy: '',
    sortBy: '',
  },
  chartTemplates: [],
  open: true,
});
