import { ResendOptionName } from '../constants';
import { INotificationModel } from './INotificationModel';
import { IUserModel } from './IUserModel';

export interface IUserNotificationModel extends IUserModel {
  notificationId: number;
  notification?: INotificationModel;
  isSubscribed: boolean;
  resend: ResendOptionName;
}
