import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

export const getBlockName = (section: IReportSectionModel) => {
  if (section.settings.sectionType === ReportSectionTypeName.Content) {
    if (section.settings.showCharts) return 'Media Analytics block';
    return 'Media block';
  } else if (section.settings.sectionType === ReportSectionTypeName.TableOfContents) {
    return 'Table of Contents block';
  } else if (section.settings.sectionType === ReportSectionTypeName.Summary) {
    if (section.settings.showCharts) return 'Media Analytics block';
    return 'Text block';
  } else return 'Unknown';
};
