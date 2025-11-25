import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

interface IGenerateReportSectionOptions {
  _id?: number;
  section?: Partial<IReportSectionModel>;
}

export const generateReportSection = (options: IGenerateReportSectionOptions) => {
  const section: IReportSectionModel = {
    id: options._id ?? 0,
    name: `Section ${options._id ?? options.section?.id ?? 0}`,
    description: '',
    sortOrder: 0,
    isEnabled: false,
    reportId: 0,
    sectionType: ReportSectionTypeName.Content,
    settings: {
      label: '',
      useAllContent: false,
      showHeadlines: false,
      showFullStory: false,
      showImage: false,
      direction: 'row',
      removeDuplicates: false,
      removeDuplicateTitles3Days: false,
      overrideExcludeHistorical: false,
      hideEmpty: false,
      groupBy: '',
      sortBy: '',
      sortDirection: '',
      url: '',
    },
    chartTemplates: [],
    ...options.section,
  };
  return section;
};
