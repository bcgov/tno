import { defaultContent, defaultReportInstanceContent } from '../constants';
import { IReportInstanceContentForm } from '../interfaces';

export const createReportInstanceContent = (
  instanceId: number,
  sectionName: string,
  userId: number,
  licenseId: number,
  mediaTypeId: number,
): IReportInstanceContentForm => {
  return {
    ...defaultReportInstanceContent,
    instanceId,
    sectionName,
    originalIndex: 0,
    content: { ...defaultContent, ownerId: userId, licenseId, mediaTypeId },
  };
};
