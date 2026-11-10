import { IReportInstanceContentModel } from '@/shared';

import { defaultContent } from './defaultContent';

export const defaultReportInstanceContent: IReportInstanceContentModel = {
  instanceId: 0,
  contentId: 0,
  content: defaultContent,
  sectionName: '',
  sortOrder: 0,
};
