import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

export const createReportSection = (
  reportId: number,
  type: ReportSectionTypeName,
): IReportSectionModel => {
  return {
    id: 0,
    reportId: reportId,
    sectionType: type,
    name: crypto.randomUUID(),
    description: '',
    sortOrder: 0,
    isEnabled: true,
    chartTemplates: [],
    settings: {
      label: '',
      useAllContent: type === ReportSectionTypeName.MediaAnalytics,
      removeDuplicates: false,
      overrideExcludeHistorical: false,
      showHeadlines: type === ReportSectionTypeName.TableOfContents,
      showFullStory: type === ReportSectionTypeName.Content,
      showImage: type === ReportSectionTypeName.Gallery,
      hideEmpty: false,
      direction:
        type === ReportSectionTypeName.Gallery || type === ReportSectionTypeName.MediaAnalytics
          ? 'row'
          : 'column',
      groupBy: 'otherSource',
      sortBy: '',
      sortDirection: '',
    },
  };
};
