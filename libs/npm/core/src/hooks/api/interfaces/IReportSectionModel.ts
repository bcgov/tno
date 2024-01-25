import { ReportSectionTypeName } from '../constants';
import {
  IFilterModel,
  IFolderModel,
  IReportModel,
  IReportSectionChartTemplateModel,
  IReportSectionSettingsModel,
  ISortableModel,
} from '.';

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
