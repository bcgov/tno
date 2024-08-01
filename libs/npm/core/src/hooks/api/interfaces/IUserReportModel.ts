import { EmailSendToName, ReportDistributionFormatName, ReportStatusName } from '../constants';
import { IUserModel } from './IUserModel';

export interface IUserReportModel {
  /** Foreign key to user */
  userId: number;
  /** User object */
  user?: IUserModel;
  /** Foreign key to report */
  reportId: number;

  /** User properties */
  username: string;
  email: string;
  preferredEmail: string;
  emailVerified: boolean;
  displayName: string;
  firstName: string;
  lastName: string;
  isEnabled: boolean;

  /** Whether the user is subscribed to this report. */
  isSubscribed: boolean;
  /** Which distribution format they want to receive. */
  format: ReportDistributionFormatName;
  /** How to send the email to this subscriber. */
  sendTo: EmailSendToName;
  /** Email status */
  status?: ReportStatusName;
  /** Email response */
  response?: any;
  /** Table row version */
  version: number;
}
