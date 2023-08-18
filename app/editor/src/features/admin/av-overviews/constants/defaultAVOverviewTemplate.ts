import { AVOverviewTemplateTypeName } from 'tno-core';

import { IAVOverviewTemplateForm } from '../interfaces';

export const defaultAVOverviewTemplate = (
  templateType: AVOverviewTemplateTypeName,
  reportTemplateId: number,
): IAVOverviewTemplateForm => ({
  templateType,
  templateId: reportTemplateId,
  sections: [],
  subscribers: [],
  isNew: true,
});
