import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IMinisterModel,
  IReportModel,
  ISystemMessageModel,
  IUserModel,
} from 'tno-core';

export interface IProfileState {
  profile?: IUserModel;
  myFilters: IFilterModel[];
  myFolders: IFolderModel[];
  myMinisters: IMinisterModel[];
  myReports: IReportModel[];
  reportsFilter: string;
  contributors: IContributorModel[];
  systemMessages: ISystemMessageModel[];
}
