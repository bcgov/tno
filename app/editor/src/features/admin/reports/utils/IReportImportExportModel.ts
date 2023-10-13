import { IReportTemplateSettingsModel } from "tno-core";
import { IReportSectionImportExportModel } from "./IReportSectionImportExportModel";

export interface IReportImportExportModel {
  name?: string;
  description?: string;
  settings: IReportTemplateSettingsModel;
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
