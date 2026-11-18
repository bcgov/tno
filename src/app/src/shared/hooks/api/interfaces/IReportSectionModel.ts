import {
  IFilterModel,
  IFolderModel,
  IReportModel,
  IReportSectionChartTemplateModel,
  IReportSectionSettingsModel,
  ISortableModel,
} from '.';
import { ReportSectionTypeName } from '../constants';

export interface IReportSectionModel extends ISortableModel<number> {
  reportId: number;
  sectionType: ReportSectionTypeName;
  folderId?: number;
  folder?: IFolderModel;
  filterId?: number;
  filter?: IFilterModel;
  linkedReportId?: number;
  linkedReport?: IReportModel;
  settings: IReportSectionSettingsModel;
  chartTemplates: IReportSectionChartTemplateModel[];
}
