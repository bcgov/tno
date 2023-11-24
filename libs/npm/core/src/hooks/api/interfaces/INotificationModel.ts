import { NotificationTypeName, ResendOptionName } from '..';
import {
  IFilterSettingsModel,
  INotificationInstanceModel,
  INotificationTemplateModel,
  ISortableModel,
  IUserModel,
} from '.';

export interface INotificationModel extends ISortableModel<number> {
  ownerId?: number;
  notificationType: NotificationTypeName;
  settings: IFilterSettingsModel;
  query: any;
  templateId: number;
  template?: INotificationTemplateModel;
  resend: ResendOptionName;
  isPublic: boolean;
  alertOnIndex: boolean;
  subscribers: IUserModel[];
  instances?: INotificationInstanceModel[];
}
