import { IReportResultForm } from 'features/my-reports/interfaces';
import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IMinisterModel,
  IReportModel,
  ISubscriberUserModel,
  ISystemMessageModel,
  IUserColleagueModel,
} from 'tno-core';

export interface IProfileState {
  impersonate?: ISubscriberUserModel;
  profile?: ISubscriberUserModel;
  filter?: IFilterModel;
  myFilters: IFilterModel[];
  myFolders: IFolderModel[];
  myMinisters: IMinisterModel[];
  myReports: IReportModel[];
  init: {
    myFilters: boolean;
    myFolders: boolean;
    myMinisters: boolean;
    myReports: boolean;
    myColleagues: boolean;
  };
  myColleagues: IUserColleagueModel[];
  reportsFilter: string;
  reportOutput?: IReportResultForm;
  reportContent: { [reportId: number]: number[] };
  contributors: IContributorModel[];
  systemMessages: ISystemMessageModel[];
}
