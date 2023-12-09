import { IContentModel, IReportInstanceContentModel } from 'tno-core';

/**
 * Converts an array of content to an array of report instance content.
 * @param content The content to convert.
 * @param report The report to convert the content for.
 * @param sectionName The section name to convert the content for.
 */
export const toInstanceContent = (
  content: IContentModel[],
  instanceId: number,
  sectionName: string,
  sortOrder: number,
): IReportInstanceContentModel[] => {
  return content.map((c) => {
    return {
      contentId: c.id,
      content: c,
      sectionName: sectionName,
      instanceId: instanceId,
      sortOrder: sortOrder,
    };
  });
};
