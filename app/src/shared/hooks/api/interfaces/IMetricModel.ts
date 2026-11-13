import { ISortableModel } from '.';

export interface IMetricModel extends ISortableModel<number> {
  reach: number;
  earned: number;
  impression: number;
}
