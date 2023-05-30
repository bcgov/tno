import { ISortableModel } from 'tno-core';

export interface IMinisterModel extends ISortableModel<number> {
  name: string;
  description: string;
}
