import {
  IFilterModel,
  IFolderContentModel,
  IFolderScheduleModel,
  IFolderSettingsModel,
  IReportModel,
  ISortableModel,
  IUserModel,
} from '.';

export interface IFolderModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  filterId?: number;
  filter?: IFilterModel;
  settings: IFolderSettingsModel;
  content: IFolderContentModel[];
  reports: IReportModel[];
  events: IFolderScheduleModel[];
}
