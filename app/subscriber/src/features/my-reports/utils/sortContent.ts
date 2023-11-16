import { IReportInstanceContentModel } from 'tno-core';

/**
 * Sorts the content by section and updates the sortOrder in each section.
 * @param content An array of content.
 * @returns A new array.
 */
export const sortContent = (content: IReportInstanceContentModel[]) => {
  var rows = [...content];
  // re-sort content into each section.
  rows = rows.sort((a, b) =>
    a.sectionName < b.sectionName ? -1 : a.sectionName > b.sectionName ? 1 : 0,
  );
  var sectionIndex = -1;
  rows.forEach((i, index) => {
    sectionIndex =
      index === 0 || rows[index - 1].sectionName !== i.sectionName ? 0 : sectionIndex + 1;
    i.sortOrder = sectionIndex;
  });
  return rows;
};
