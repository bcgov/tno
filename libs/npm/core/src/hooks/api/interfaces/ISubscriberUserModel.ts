import { UserStatusName } from '../constants';
import {
  IAuditColumnsModel,
  IFilterModel,
  IFolderModel,
  INotificationModel,
  IOrganizationModel,
  IReportInstanceModel,
  IReportModel,
  IUserColleagueModel,
  IUserPreferencesModel,
} from '.';

export interface ISubscriberUserModel extends IAuditColumnsModel {
  id: number;
  key: string;
  username: string;
  email: string;
  preferredEmail: string;
  displayName: string;
  firstName: string;
  lastName: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
  status: UserStatusName;
  preferences?: IUserPreferencesModel;
  note: string;
  roles?: string[];
  organizations?: IOrganizationModel[];
  folders?: IFolderModel[];
  filters?: IFilterModel[];
  reports?: IReportModel[];
  reportInstances?: IReportInstanceModel[];
  notifications?: INotificationModel[];
  colleagues?: IUserColleagueModel[];
  sources?: number[];
  mediaTypes?: number[];
}
