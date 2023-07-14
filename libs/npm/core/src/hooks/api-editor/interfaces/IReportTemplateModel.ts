import { IChartTemplateModel, IReportTemplateSettingsModel, ISortableModel } from '.';

export interface IReportTemplateModel extends ISortableModel<number> {
  subject: string;
  body: string;
  settings: IReportTemplateSettingsModel;
  chartTemplates: IChartTemplateModel[];
}
