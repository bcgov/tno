import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

export const getBlockName = (section: IReportSectionModel) => {
  if (section.settings.sectionType === ReportSectionTypeName.Content) {
    if (section.settings.showCharts) return 'Media Analytics';
    return 'Media Stories';
  } else if (section.settings.sectionType === ReportSectionTypeName.TableOfContents) {
    return 'Table of Contents';
  } else if (section.settings.sectionType === ReportSectionTypeName.Summary) {
    if (section.settings.showCharts) return 'Media Analytics';
    return 'Text';
  } else return 'Unknown';
};
