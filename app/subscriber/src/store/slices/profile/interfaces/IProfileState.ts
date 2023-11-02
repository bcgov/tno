import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IMinisterModel,
  ISystemMessageModel,
  IUserModel,
} from 'tno-core';

export interface IProfileState {
  profile?: IUserModel;
  myFilters: IFilterModel[];
  myFolders: IFolderModel[];
  myMinisters: IMinisterModel[];
  contributors: IContributorModel[];
  systemMessages: ISystemMessageModel[];
}
