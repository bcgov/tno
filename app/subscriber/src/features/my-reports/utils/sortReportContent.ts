import { IReportInstanceContentModel } from 'tno-core';

import { IReportForm, IReportInstanceContentForm } from '../interfaces';

export const sortReportContent = (
  report: IReportForm,
  content: IReportInstanceContentModel[],
): IReportInstanceContentForm[] => {
  // Re-sort content in each section
  var allContent: IReportInstanceContentModel[] = [];
  [...report.sections]
    .sort((a, b) => (a.sortOrder < b.sortOrder ? -1 : a.sortOrder > b.sortOrder ? 1 : 0))
    .forEach((section) => {
      allContent = [
        ...allContent,
        ...content
          .filter((c) => c.sectionName === section.name)
          .sort((a, b) => (a.sortOrder < b.sortOrder ? -1 : a.sortOrder > b.sortOrder ? 1 : 0))
          .map((c) => ({ ...c, sortOrder: c.sortOrder })),
      ];
    });

  return allContent.map<IReportInstanceContentForm>((c, i) => ({ ...c, originalIndex: i }));
};
