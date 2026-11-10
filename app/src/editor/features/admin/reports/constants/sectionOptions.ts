import { OptionItem, ReportSectionTypeName } from 'tno-core';

export const sectionOptions = [
  new OptionItem('Table of Contents', ReportSectionTypeName.TableOfContents),
  new OptionItem('Content', ReportSectionTypeName.Content),
  new OptionItem('Text', ReportSectionTypeName.Text),
  new OptionItem('Media Analytics', ReportSectionTypeName.MediaAnalytics),
  new OptionItem('Gallery', ReportSectionTypeName.Gallery),
];
