import { ISortableModel } from 'tno-core';

export interface IMinisterModel extends ISortableModel<number> {
  name: string;
  aliases: string;
  description: string;
}
