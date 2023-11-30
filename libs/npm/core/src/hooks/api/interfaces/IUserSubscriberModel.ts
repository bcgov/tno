import { IUserInfoModel } from '.';

export interface IUserSubscriberModel extends IUserInfoModel {
  isSubscribed: boolean;
  requestedIsSubscribedStatus?: boolean;
  subscriptionChangeActioned?: boolean;
}
