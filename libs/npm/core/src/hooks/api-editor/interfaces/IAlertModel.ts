import { ISortableModel } from '.';

export interface ISystemMessageModel extends ISortableModel<number> {
  message: string;
}
