import { IFolderContentModel, IFolderSettingsModel, ISortableModel, IUserModel } from '.';

export interface IFolderModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  settings: IFolderSettingsModel;
  content: IFolderContentModel[];
}
