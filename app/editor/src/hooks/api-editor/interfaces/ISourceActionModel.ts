import { ISortableModel } from '.';

export interface ISourceActionModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
  value: boolean;
}
