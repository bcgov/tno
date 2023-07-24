import { ISortableModel } from 'tno-core';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  position: string;
}
