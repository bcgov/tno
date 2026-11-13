import {
  getSortableOptions,
  OptionItem,
  ReportTypeName,
  type IReportTemplateModel,
} from '@/shared';

export const getReportTemplateOptions = (
  reportTemplates: IReportTemplateModel[],
  currentId: number,
) => {
  return getSortableOptions(
    reportTemplates.filter((t) => t.reportType === ReportTypeName.Content),
    currentId,
    [new OptionItem('[New Template]', 0)],
  );
};
