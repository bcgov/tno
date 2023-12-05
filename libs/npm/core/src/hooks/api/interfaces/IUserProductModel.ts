import { IUserModel } from '.';

export interface IUserProductModel extends IUserModel {
  /** Whether the user is subscribed to this product. */
  isSubscribed: boolean;
  /** Whether the user has requested to be subscribed. */
  requestedIsSubscribedStatus?: boolean; // TODO: I find this status confusing
  /** Whether the user has requested a change to their subscription. */
  subscriptionChangeActioned?: boolean; // TODO: I find this status confusing
}
