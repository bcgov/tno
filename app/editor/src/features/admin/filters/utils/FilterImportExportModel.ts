import { IFilterImportExportModel } from './IFilterImportExportModel';

export class FilterImportExportModel implements IFilterImportExportModel {
  name?: string | undefined;
  description?: string | undefined;
  settings: any;
}
