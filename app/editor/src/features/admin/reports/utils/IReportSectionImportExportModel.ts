import { IReportSectionChartTemplateModel, IReportSectionSettingsModel } from 'tno-core';

export interface IReportSectionImportExportModel {
  name: string;
  description?: string;
  sortOrder: number;
  isEnabled: boolean;
  folderName?: string;
  filterName?: string;
  settings: IReportSectionSettingsModel;
  chartTemplates: IReportSectionChartTemplateModel[];
}
