import { IReportTemplateModel, OptionItem, ReportTypeName } from 'tno-core';

export const getReportTemplateOptions = (templates: IReportTemplateModel[]) => {
  return templates
    .filter((t) => t.reportType === ReportTypeName.AVOverview && t.isEnabled)
    .map((t) => new OptionItem(t.name, t.id));
};
