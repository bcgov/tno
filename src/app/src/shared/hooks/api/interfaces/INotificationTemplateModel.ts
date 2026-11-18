import { ISortableModel } from '.';

export interface INotificationTemplateModel extends ISortableModel<number> {
  subject: string;
  body: string;
  isPublic: boolean;
  settings: any;
}
