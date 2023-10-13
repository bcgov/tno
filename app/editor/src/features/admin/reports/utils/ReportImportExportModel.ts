import { IReportImportExportModel } from './IReportImportExportModel';

export class ReportImportExportModel implements IReportImportExportModel {
  name?: string | undefined;
  description?: string | undefined;
  settings: any;
}
