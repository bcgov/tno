import { ReportTypeName } from '..';
import { IChartTemplateModel, IReportTemplateSettingsModel, ISortableModel } from '.';

export interface IReportTemplateModel extends ISortableModel<number> {
  reportType: ReportTypeName;
  subject: string;
  body: string;
  settings: IReportTemplateSettingsModel;
  chartTemplates: IChartTemplateModel[];
}
