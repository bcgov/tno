import { ISortableModel } from '.';

export interface ISentimentModel extends ISortableModel<number> {
  value: number;
  rate: number;
}
