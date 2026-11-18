import { NotificationTypeName } from '../constants';
import { ISortPageFilter } from './ISortPageFilter';

export interface INotificationFilter extends ISortPageFilter {
  ownerId?: number;
  alertOnIndex?: boolean;
  notificationType?: NotificationTypeName;
  subscriberUserId?: number;
}
