import { ResendOptionName } from '../constants';
import { IUserModel } from './IUserModel';

export interface IUserNotificationModel extends IUserModel {
  isSubscribed: boolean;
  resend: ResendOptionName;
}
