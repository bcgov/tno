import { IFolderModel, IMinisterModel, ISystemMessageModel, IUserModel } from 'tno-core';

export interface IProfileState {
  profile?: IUserModel;
  myFolders: IFolderModel[];
  myMinisters: IMinisterModel[];
  systemMessages: ISystemMessageModel[];
}
