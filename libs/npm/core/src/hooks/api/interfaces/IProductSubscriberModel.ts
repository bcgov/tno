import { ISortableModel } from '.';

export interface IProductSubscriberModel extends ISortableModel<number> {
  isSubscribed: boolean;
  requestedIsSubscribedStatus?: boolean;
  subscriptionChangeActioned?: boolean;
}
