import { IReportSectionChartTemplateModel, IReportSectionSettingsModel } from "tno-core";

export interface IReportSectionImportExportModel {
  folderName?: string;
  filterName?: string;
  settings: IReportSectionSettingsModel;
  chartTemplates: IReportSectionChartTemplateModel[];
}
