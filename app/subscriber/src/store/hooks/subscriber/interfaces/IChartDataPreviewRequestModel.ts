import { IChartSectionSettingsModel } from 'tno-core';

export interface IChartDataPreviewRequestModel {
  /** The chart settings the form currently holds. */
  sectionSettings: IChartSectionSettingsModel;
  /** Whether the chart counts the stories listed in this report. */
  useAllContent: boolean;
  /** The section's own filter. */
  filterId?: number;
  /** The section's own folder. */
  folderId?: number;
  /** The linked report. */
  linkedReportId?: number;
  /** The unsent instance to take stories from. Defaults to the report's current instance. */
  instanceId?: number;
}
