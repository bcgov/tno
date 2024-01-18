import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

/**
 * Determine if all sections that can contain content are hidden.
 * @param sections An array of report sections.
 * @returns True if all 'relevant' sections are hidden when empty.
 */
export const getHideEmpty = (sections: IReportSectionModel[]) => {
  if (!sections.length) return false;
  const hideEmpty = !sections.some(
    (section) =>
      !section.settings.hideEmpty &&
      (section.sectionType === ReportSectionTypeName.Content ||
        section.sectionType === ReportSectionTypeName.Gallery ||
        (section.sectionType === ReportSectionTypeName.MediaAnalytics &&
          (!!section.filterId || !!section.folderId))),
  );
  return hideEmpty;
};
