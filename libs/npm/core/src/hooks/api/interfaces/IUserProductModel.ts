import { ReportDistributionFormatName } from 'tno-core';

import { EmailSendToName } from '../constants';
import { IUserModel } from '.';

export interface IUserProductModel extends IUserModel {
  /** Whether the user is subscribed to this product. */
  isSubscribed: boolean;
  /** Which distribution format they want to receive. */
  format?: ReportDistributionFormatName;
  /** How to send the email to this subscriber. */
  sendTo: EmailSendToName;

  // TODO: These are for subscribers requesting to receive a product.
  /** Whether the user has requested to be subscribed. */
  requestedIsSubscribedStatus?: boolean; // TODO: I find this status confusing
  /** Whether the user has requested a change to their subscription. */
  subscriptionChangeActioned?: boolean; // TODO: I find this status confusing
}
