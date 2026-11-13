import { OptionItem, ReportTypeName, type IReportTemplateModel } from '@/shared';

export const getReportTemplateOptions = (templates: IReportTemplateModel[]) => {
  return templates
    .filter((t) => t.reportType === ReportTypeName.AVOverview && t.isEnabled)
    .map((t) => new OptionItem(t.name, t.id));
};
