import { IChartTemplateSettingsModel, ISortableModel } from '.';

export interface IChartTemplateModel extends ISortableModel<number> {
  isPublic: boolean;
  template: string;
  settings: IChartTemplateSettingsModel;
}
