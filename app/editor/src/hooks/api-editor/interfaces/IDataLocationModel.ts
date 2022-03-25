import { ISortableModel } from '.';

export interface IDataLocationModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
}
