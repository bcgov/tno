import { IReportResultForm } from 'features/my-reports/interfaces';
import {
  IContributorModel,
  IFilterModel,
  IFolderModel,
  IReportModel,
  ISubscriberUserModel,
  ISystemMessageModel,
  ITonePoolModel,
  IUserColleagueModel,
} from 'tno-core';

export interface IProfileState {
  impersonate?: ISubscriberUserModel;
  profile?: ISubscriberUserModel;
  filter?: IFilterModel;
  myFilters: IFilterModel[];
  myFolders: IFolderModel[];
  myReports: IReportModel[];
  myColleagues: IUserColleagueModel[];
  // There init values are used to ensure we only initialize once.
  init: {
    myFilters: boolean;
    myFolders: boolean;
    myReports: boolean;
    myColleagues: boolean;
    myTonePool: boolean;
  };
  reportsFilter: string;
  reportOutput?: IReportResultForm;
  reportContent: { [reportId: number]: number[] };
  contributors: IContributorModel[];
  messages: ISystemMessageModel[];
  myTonePool: ITonePoolModel;
}
