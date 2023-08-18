import { IAVOverviewInstanceModel, IAVOverviewTemplateModel } from 'tno-core';

export const defaultAVOverviewInstance = (
  template: IAVOverviewTemplateModel,
  publishedOn: Date | string,
): IAVOverviewInstanceModel => ({
  id: 0,
  templateType: template.templateType,
  publishedOn,
  isPublished: false,
  response: undefined,
  sections: [],
});
