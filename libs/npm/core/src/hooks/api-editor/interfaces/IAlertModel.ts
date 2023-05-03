import { ISortableModel } from '.';

export interface IAlertModel extends ISortableModel<number> {
  message: string;
}
