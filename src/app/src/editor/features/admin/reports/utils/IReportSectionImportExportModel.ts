import { type IReportSectionChartTemplateModel, type IReportSectionSettingsModel } from '@/shared';

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
