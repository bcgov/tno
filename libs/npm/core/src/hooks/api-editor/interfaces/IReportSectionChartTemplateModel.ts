import { IReportSectionChartTemplateSettingsModel, ISortableModel } from '.';

export interface IReportSectionChartTemplateModel extends ISortableModel<number> {
  isPublic: boolean;
  template: string;
  settings: IReportSectionChartTemplateSettingsModel;
}
