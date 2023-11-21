import { IReportInstanceContentModel } from 'tno-core';

import { defaultContent, defaultReportInstanceContent } from '../constants';

export const createReportInstanceContent = (
  instanceId: number,
  sectionName: string,
  userId: number,
  licenseId: number,
  mediaTypeId: number,
): IReportInstanceContentModel => {
  return {
    ...defaultReportInstanceContent,
    instanceId,
    sectionName,
    content: { ...defaultContent, ownerId: userId, licenseId, mediaTypeId },
  };
};
