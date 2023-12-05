import { IUserModel } from './IUserModel';

export interface IUserAVOverviewModel extends IUserModel {
  /** Whether the user is subscribed to this report. */
  isSubscribed: boolean;
}
