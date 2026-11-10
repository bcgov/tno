import { AVOverviewTemplateTypeName, type IAVOverviewTemplateModel } from '@/shared';

export const defaultAVOverviewTemplate: IAVOverviewTemplateModel = {
  templateType: AVOverviewTemplateTypeName.Weekday,
  templateId: 0,
  sections: [],
  subscribers: [],
};
