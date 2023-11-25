import { ReportSectionTypeName } from 'tno-core';

import { IReportSectionForm } from '../interfaces';

export const defaultReportSection = (
  type: ReportSectionTypeName,
  sortOrder: number,
  showCharts: boolean = false,
  showHeadlines: boolean | undefined = undefined,
  showFullStory: boolean | undefined = undefined,
  hideEmpty: boolean = false,
): IReportSectionForm => ({
  id: 0,
  name: crypto.randomUUID(),
  description: '',
  sortOrder: sortOrder,
  isEnabled: true,
  reportId: 0,
  settings: {
    label:
      type === ReportSectionTypeName.TableOfContents
        ? 'Table of Contents'
        : type === ReportSectionTypeName.Summary
        ? 'Executive Summary'
        : '',
    sectionType: type,
    showHeadlines: showHeadlines ?? type === ReportSectionTypeName.TableOfContents,
    showFullStory: showFullStory ?? type === ReportSectionTypeName.Content,
    showImage: false,
    showCharts: showCharts,
    chartsOnTop: false,
    chartDirection: 'row',
    removeDuplicates: false,
    hideEmpty:
      type === ReportSectionTypeName.TableOfContents || type === ReportSectionTypeName.Summary
        ? false
        : hideEmpty,
    groupBy: '',
    sortBy: '',
    orderByField: '',
  },
  chartTemplates: [],
  expand: true,
});
