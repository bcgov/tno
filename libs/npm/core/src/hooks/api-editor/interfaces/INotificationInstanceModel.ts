import { IAuditColumnsModel } from '.';

export interface INotificationInstanceModel extends IAuditColumnsModel {
  id: number;
  notificationId: number;
  contentId: number;
  response: any;
}
