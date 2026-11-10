import { NotificationTypeName, ResendOptionName } from '..';
import {
  IFilterSettingsModel,
  INotificationInstanceModel,
  INotificationTemplateModel,
  ISortableModel,
  IUserModel,
  IUserNotificationModel,
} from '.';

export interface INotificationModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  notificationType: NotificationTypeName;
  settings: IFilterSettingsModel;
  query: any;
  templateId: number;
  template?: INotificationTemplateModel;
  resend: ResendOptionName;
  isPublic: boolean;
  alertOnIndex: boolean;
  subscribers: IUserNotificationModel[];
  instances?: INotificationInstanceModel[];
}
