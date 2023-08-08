import { IChartSectionSettingsModel, IChartTemplateSettingsModel, ISortableModel } from '.';

export interface IReportSectionChartTemplateModel extends ISortableModel<number> {
  /** Whether this chart template is available to all users. */
  isPublic: boolean;
  /** Razor syntax template used to generate JSON data. */
  template: string;
  /** Chart template default configuration settings. */
  settings: IChartTemplateSettingsModel;
  /** Configuration settings specified for the chart within this section. */
  sectionSettings: IChartSectionSettingsModel;
}
