import { ISortableModel } from '.';

export interface IActionModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
  value?: string;
}
