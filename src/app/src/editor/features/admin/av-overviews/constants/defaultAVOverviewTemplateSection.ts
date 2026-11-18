import { type AVOverviewTemplateTypeName, type IAVOverviewTemplateSectionModel } from '@/shared';

export const defaultAVOverviewTemplateSection = (
  templateType: AVOverviewTemplateTypeName,
): IAVOverviewTemplateSectionModel => ({
  id: 0,
  name: '',
  templateType,
  otherSource: '',
  anchors: '',
  startTime: '',
  items: [],
  sortOrder: 0,
});
