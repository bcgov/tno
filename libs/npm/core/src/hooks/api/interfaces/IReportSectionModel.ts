import {
  IFilterModel,
  IFolderModel,
  IReportSectionChartTemplateModel,
  IReportSectionSettingsModel,
  ISortableModel,
} from '.';

export interface IReportSectionModel extends ISortableModel<number> {
  reportId: number;
  folderId?: number;
  folder?: IFolderModel;
  filterId?: number;
  filter?: IFilterModel;
  settings: IReportSectionSettingsModel;
  chartTemplates: IReportSectionChartTemplateModel[];
}
