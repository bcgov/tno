import {
  EmailSendToName,
  ProductRequestStatusName,
  ReportDistributionFormatName,
  UserAccountTypeName,
} from '../constants';
import { IAuditColumnsModel } from './IAuditColumnsModel';
import { IProductModel } from './IProductModel';

export interface IUserProductModel extends IAuditColumnsModel {
  /** Foreign key to user */
  userId: number;
  /** Foreign key to report */
  productId: number;
  /** The product. */
  product?: IProductModel;
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
}
