import {
  EmailSendToName,
  ReportDistributionFormatName,
  ReportStatusName,
  UserAccountTypeName,
} from '../constants';
import { IAuditColumnsModel } from './IAuditColumnsModel';
import { IReportModel } from './IReportModel';
import { IUserModel } from './IUserModel';

export interface IUserReportModel extends IAuditColumnsModel {
  /** Foreign key to user */
  userId: number;
  /** User object */
  user?: IUserModel;
  /** Foreign key to report */
  reportId: number;
  /** The report. */
  report?: IReportModel;

  /** User properties */
  username: string;
  email: string;
  preferredEmail: string;
  emailVerified: boolean;
  displayName: string;
  firstName: string;
  lastName: string;
  isEnabled: boolean;
  accountType: UserAccountTypeName;

  /** Whether the user is subscribed to this report. */
  isSubscribed: boolean;
  /** Which distribution format they want to receive. */
  format: ReportDistributionFormatName;
  /** How to send the email to this subscriber. */
  sendTo: EmailSendToName;
  /** Email status */
  linkStatus?: ReportStatusName;
  /** Email response */
  linkResponse?: any;
  /** Email status */
  textStatus?: ReportStatusName;
  /** Email response */
  textResponse?: any;
}
