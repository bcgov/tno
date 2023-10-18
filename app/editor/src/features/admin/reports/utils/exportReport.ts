import { IReportSectionModel, IReportSettingsModel, IReportTemplateModel } from 'tno-core';

import { IReportImportExportModel } from './IReportImportExportModel';
import { IReportSectionImportExportModel } from './IReportSectionImportExportModel';
import { IReportTemplateImportExportModel } from './IReportTemplateImportExportModel';

export const exportReport = (
  name: string,
  description: string,
  isEnabled: boolean,
  isPublic: boolean,
  settings: IReportSettingsModel,
  sections: IReportSectionModel[],
  template?: IReportTemplateModel,
): IReportImportExportModel => {
  var exportReport = {} as IReportImportExportModel;
  exportReport.name = name;
  exportReport.description = description;
  exportReport.isEnabled = isEnabled;
  exportReport.isPublic = isPublic;
  exportReport.settings = settings;
  if (template) {
    exportReport.template = {} as IReportTemplateImportExportModel;
    exportReport.template.name = template.name;
    exportReport.template.settings = template.settings;
  }
  exportReport.sections = [] as Array<IReportSectionImportExportModel>;
  sections.forEach((section) => {
    var exportSection = {} as IReportSectionImportExportModel;
    exportSection.sortOrder = section.sortOrder;
    exportSection.isEnabled = section.isEnabled;
    exportSection.settings = section.settings;
    exportSection.chartTemplates = section.chartTemplates;
    if (section.filterId !== null) exportSection.filterName = section.filter?.name;
    if (section.folderId !== null) exportSection.folderName = section.folder?.name;
    exportReport.sections.push(exportSection);
  });

  return exportReport;
};
