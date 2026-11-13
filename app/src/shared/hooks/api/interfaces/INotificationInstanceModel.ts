import { IAuditColumnsModel, INotificationModel } from '.';
import { NotificationStatusName } from '../constants';

export interface INotificationInstanceModel extends IAuditColumnsModel {
  id: number;
  notificationId: number;
  notification: INotificationModel;
  contentId: number;
  ownerId?: number;
  sentOn?: string;
  status: NotificationStatusName;
  response: any;
  subject: string;
}
