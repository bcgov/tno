import { IContentModel, IReportInstanceContentModel, IReportModel } from 'tno-core';

/**
 * Converts an array of content to an array of report instance content.
 * @param content The content to convert.
 * @param report The report to convert the content for.
 * @param sectionName The section name to convert the content for.
 */
export const toInstanceContent = (
  content: IContentModel[],
  report: IReportModel,
  sectionName: string,
): IReportInstanceContentModel[] => {
  return content.map((c) => {
    return {
      contentId: c.id,
      content: c,
      sectionName: sectionName,
      instanceId: !!report.instances.length ? report.instances[report.instances.length - 1].id : 1,
      createdBy: c.createdBy,
      createdOn: c.createdOn,
      sortOrder: 0,
    };
  });
};
