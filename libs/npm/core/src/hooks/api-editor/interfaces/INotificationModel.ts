import { NotificationTypeName, ResendOptionName } from '..';
import { ISortableModel, IUserModel } from '.';

export interface INotificationModel extends ISortableModel<number> {
  ownerId: number;
  notificationType: NotificationTypeName;
  requireAlert: boolean;
  settings: any;
  filter: any;
  template: string;
  resend: ResendOptionName;
  isPublic: boolean;
  subscribers: IUserModel[];
}
