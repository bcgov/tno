import { IContentModel, IReportInstanceContentModel } from 'tno-core';

export type MinimalReportInstanceContent = Pick<IReportInstanceContentModel, 'instanceId' | 'contentId' | 'sectionName' | 'sortOrder'>;

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

/**
 * Converts content into a lightweight payload for the fast append endpoint.
 * Drops the nested content graph to trim the request size.
 */
export const toInstanceContentPayload = (
  content: IContentModel[],
  instanceId: number,
  sectionName: string,
  sortOrder: number,
): MinimalReportInstanceContent[] => {
  return content.map((c) => ({
    contentId: c.id,
    sectionName,
    instanceId,
    sortOrder,
  }));
};
