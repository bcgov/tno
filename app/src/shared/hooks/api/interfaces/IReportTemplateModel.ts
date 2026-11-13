import { IChartTemplateModel, IReportTemplateSettingsModel, ISortableModel } from '.';
import { ReportTypeName } from '..';

export interface IReportTemplateModel extends ISortableModel<number> {
  reportType: ReportTypeName;
  subject: string;
  body: string;
  isPublic: boolean;
  settings: IReportTemplateSettingsModel;
  chartTemplates: IChartTemplateModel[];
}
