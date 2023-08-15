import { AVOverviewTemplateTypeName, IAVOverviewTemplateSectionModel } from 'tno-core';

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
