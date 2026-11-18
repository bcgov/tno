import { IAuditColumnsModel, IUserModel } from '..';

export interface IUserContentNotificationModel extends IAuditColumnsModel {
  userId: number;
  user?: IUserModel;
  contentId: number;
  isSubscribed: boolean;
}
