import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IMinisterModel,
  IReportModel,
  IReportResultModel,
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
  reportOutput?: IReportResultModel;
  contributors: IContributorModel[];
  systemMessages: ISystemMessageModel[];
}
