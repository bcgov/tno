import {
  EmailSendToName,
  ProductRequestStatusName,
  ReportDistributionFormatName,
  UserAccountTypeName,
} from '../constants';

export interface IUserProductModel {
  /** Foreign key to user */
  userId: number;
  /** Foreign key to report */
  productId: number;
  /** The status of the user product. */
  status: ProductRequestStatusName;

  /** Whether the user is subscribed to this report. */
  isSubscribed: boolean;
  /** Which distribution format they want to receive. */
  format?: ReportDistributionFormatName;
  /** How to send the email to this subscriber. */
  sendTo?: EmailSendToName;

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

  /** Table row version */
  version?: number;
}
