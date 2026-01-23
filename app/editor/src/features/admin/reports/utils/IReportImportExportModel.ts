import { type IReportSettingsModel } from 'tno-core';

import { type IReportSectionImportExportModel } from './IReportSectionImportExportModel';
import { type IReportTemplateImportExportModel } from './IReportTemplateImportExportModel';

export interface IReportImportExportModel {
  name: string;
  description?: string;
  isEnabled: boolean;
  isPublic: boolean;
  template: IReportTemplateImportExportModel;
  settings: IReportSettingsModel;
  sections: IReportSectionImportExportModel[];
}

// export class ReportImportExportModel implements IReportImportExportModel {
//   name?: string | undefined;
//   description?: string | undefined;
//   settings: IReportTemplateSettingsModel;
//   sections: IReportSectionImportExportModel[];
//   // name?: string;
//   // description?: string;
//   // settings: IReportTemplateSettingsModel;
//   // sections: IReportSectionImportExportModel[];
// }
