import { IReportInstanceContentModel } from 'tno-core';

/**
 * Sorts the content by section and updates the sortOrder in each section.
 * @param content An array of content.
 * @param updateSortOrder Whether to update sort order.
 * @returns A new array.
 */
export const sortContent = (
  content: IReportInstanceContentModel[],
  updateSortOrder: boolean = false,
): IReportInstanceContentModel[] => {
  var rows = [...content];
  // re-sort content into each section.
  rows = rows.sort((a, b) =>
    a.sectionName < b.sectionName ? -1 : a.sectionName > b.sectionName ? 1 : 0,
  );
  var sectionIndex = -1;
  const results = rows.map((i, index) => {
    sectionIndex =
      index === 0 || rows[index - 1].sectionName !== i.sectionName ? 0 : sectionIndex + 1;
    return {
      ...i,
      sortOrder: updateSortOrder ? sectionIndex : i.sortOrder,
    };
  });
  return results;
};
