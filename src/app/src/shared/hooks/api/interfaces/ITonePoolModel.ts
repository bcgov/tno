import { ISortableModel } from '.';

export interface ITonePoolModel extends ISortableModel<number> {
  ownerId: number;
  isPublic: boolean;
}
