import {
  type IFilterModel,
  type IFolderModel,
  type IReportModel,
  type IReportTemplateModel,
  ReportSectionTypeName,
} from 'tno-core';

import { defaultReport } from '../constants';
import { createReportSection } from './createReportSection';
import { type IReportImportExportModel } from './IReportImportExportModel';

export const parseExportedReport = (
  value: IReportImportExportModel,
  templates: IReportTemplateModel[],
  filters: IFilterModel[],
  folders: IFolderModel[],
): IReportModel => {
  const importedModel = { ...defaultReport } as IReportModel;
  importedModel.name = value.name;
  importedModel.description = value.description ?? '';
  importedModel.isEnabled = value.isEnabled;
  importedModel.isPublic = value.isPublic;
  importedModel.settings = value.settings;
  const template = templates.find((rt) => rt.name === value.template.name);
  if (template) {
    importedModel.template = template;
    importedModel.templateId = template.id;
  } else {
    throw new Error(`Could not find Template with name [${value.template.name}]`);
  }

  if (value.sections?.length) {
    importedModel.sections = [];
    value.sections.forEach((section) => {
      const parsedSection = createReportSection(0, ReportSectionTypeName.Content);
      parsedSection.description = section.description ?? '';
      parsedSection.isEnabled = section.isEnabled;
      parsedSection.sortOrder = section.sortOrder;
      parsedSection.settings = section.settings;
      parsedSection.chartTemplates = section.chartTemplates;
      if (section.filterName) {
        const filter = filters.find((f) => f.name === section.filterName);
        if (filter) {
          parsedSection.filter = filter;
          parsedSection.filterId = filter.id;
        } else {
          throw new Error(`Could not find Filter with name [${section.filterName}]`);
        }
      }
      if (section.folderName) {
        const folder = folders.find((f) => f.name === section.folderName);
        if (folder) {
          parsedSection.folder = folder;
          parsedSection.folderId = folder.id;
        } else {
          throw new Error(`Could not find Folder with name [${section.folderName}]`);
        }
      }
      importedModel.sections.push(parsedSection);
    });
  }

  /// zero these out as we aren't handling this yet...
  importedModel.subscribers = [];
  importedModel.events = [];
  importedModel.instances = [];

  return importedModel;
};
