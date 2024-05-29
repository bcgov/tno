import { EmailSendToName, ReportDistributionFormatName } from '../constants';

export interface IUserReportModel {
  /** Foreign key to user */
  userId: number;
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
  /** Table row version */
  version: number;
}
