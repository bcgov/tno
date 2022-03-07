import { ISortableModel } from '.';

export interface ITonePoolModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
}
