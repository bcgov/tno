import {
  IFilterModel,
  IFolderContentModel,
  IFolderSettingsModel,
  IReportModel,
  IScheduleModel,
  ISortableModel,
  IUserModel,
} from '.';

export interface IFolderModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  scheduleId?: number;
  schedule?: IScheduleModel;
  filterId?: number;
  filter?: IFilterModel;
  settings: IFolderSettingsModel;
  content: IFolderContentModel[];
  reports: IReportModel[];
}
