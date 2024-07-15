import { UserAccountTypeName, UserStatusName } from '../constants';
import {
  IAuditColumnsModel,
  IFilterModel,
  IFolderModel,
  INotificationModel,
  IOrganizationModel,
  IProductModel,
  IReportInstanceModel,
  IReportModel,
  IUserColleagueModel,
  IUserPreferencesModel,
} from '.';

export interface IUserModel extends IAuditColumnsModel {
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
  emailVerified: boolean;
  isSystemAccount: boolean;
  accountType: UserAccountTypeName;
  preferences?: IUserPreferencesModel;
  uniqueLogins: number;
  note: string;
  roles?: string[];
  organizations?: IOrganizationModel[];
  products?: IProductModel[];
  folders?: IFolderModel[];
  filters?: IFilterModel[];
  reports?: IReportModel[];
  reportInstances?: IReportInstanceModel[];
  notifications?: INotificationModel[];
  colleagues?: IUserColleagueModel[];
  sources?: number[];
  mediaTypes?: number[];
}
