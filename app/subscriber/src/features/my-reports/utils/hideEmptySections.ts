import { ReportSectionTypeName } from 'tno-core';

import { IReportForm } from '../interfaces';

/**
 * Changes the value of each report section hideEmpty property based on the 'checked' value.
 * @param report Report form.
 * @param checked Whether the hideEmpty is checked.
 * @returns An new report form.
 */
export const hideEmptySections = (report: IReportForm, checked: boolean) => {
  return {
    ...report,
    hideEmptySections: checked,
    sections: report.sections.map((section) => ({
      ...section,
      settings: {
        ...section.settings,
        hideEmpty:
          section.sectionType === ReportSectionTypeName.Text ||
          section.sectionType === ReportSectionTypeName.MediaAnalytics ||
          section.sectionType === ReportSectionTypeName.TableOfContents
            ? false
            : checked,
      },
    })),
  };
};
