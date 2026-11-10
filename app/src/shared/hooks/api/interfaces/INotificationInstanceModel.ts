import { NotificationStatusName } from '../constants';
import { IAuditColumnsModel, INotificationModel } from '.';

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
