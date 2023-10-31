import { NotificationStatusName } from '../constants';
import { IAuditColumnsModel } from '.';

export interface INotificationInstanceModel extends IAuditColumnsModel {
  id: number;
  notificationId: number;
  contentId: number;
  ownerId?: number;
  sentOn?: string;
  status: NotificationStatusName;
  response: any;
}
