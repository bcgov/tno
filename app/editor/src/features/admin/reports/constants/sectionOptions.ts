import { OptionItem, ReportSectionTypeName } from 'tno-core';

export const sectionOptions = [
  new OptionItem('Content', ReportSectionTypeName.Content),
  new OptionItem('Table of Contents', ReportSectionTypeName.TableOfContents),
  new OptionItem('Summary', ReportSectionTypeName.Summary),
];
