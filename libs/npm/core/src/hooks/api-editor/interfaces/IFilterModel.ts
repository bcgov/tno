import { IFilterSettingsModel, ISortableModel, IUserModel } from '.';

export interface IFilterModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  query: any;
  settings: IFilterSettingsModel;
}
