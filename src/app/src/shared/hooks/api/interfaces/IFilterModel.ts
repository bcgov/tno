import { IFilterSettingsModel, IFolderModel, IReportModel, ISortableModel, IUserModel } from '.';

export interface IFilterModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  query: any;
  settings: IFilterSettingsModel;
  /** The AI prompt that was used to generate this filter. */
  prompt?: string;
  reports: IReportModel[];
  folders: IFolderModel[];
}
