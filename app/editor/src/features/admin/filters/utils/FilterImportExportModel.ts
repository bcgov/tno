import { type IFilterImportExportModel } from './IFilterImportExportModel';

export class FilterImportExportModel implements IFilterImportExportModel {
  name?: string | undefined;
  description?: string | undefined;
  prompt?: string | undefined;
  settings: any;
}
