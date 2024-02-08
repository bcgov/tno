import { IReportResultForm } from 'features/my-reports/interfaces';
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
  filter?: IFilterModel;
  myFilters: IFilterModel[];
  myFolders: IFolderModel[];
  myMinisters: IMinisterModel[];
  myReports: IReportModel[];
  reportsFilter: string;
  reportOutput?: IReportResultForm;
  reportContent: { [reportId: number]: number[] };
  contributors: IContributorModel[];
  systemMessages: ISystemMessageModel[];
}
