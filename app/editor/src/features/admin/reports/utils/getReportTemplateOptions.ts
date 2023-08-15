import { getSortableOptions, IReportTemplateModel, OptionItem, ReportTypeName } from 'tno-core';

export const getReportTemplateOptions = (reportTemplates: IReportTemplateModel[]) => {
  return getSortableOptions(
    reportTemplates.filter((t) => t.reportType === ReportTypeName.Content),
    [new OptionItem('[New Template]', 0)],
  );
};
