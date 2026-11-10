import { AVOverviewTemplateType, AVOverviewTemplateTypeName } from 'tno-core';

export const getAVOverviewTemplateTypeName = (templateType: AVOverviewTemplateType) => {
  switch (templateType) {
    case AVOverviewTemplateType.Weekday:
      return AVOverviewTemplateTypeName.Weekday;
    case AVOverviewTemplateType.Weekend:
    default:
      return AVOverviewTemplateTypeName.Weekend;
  }
};
