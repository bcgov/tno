import { ISortableModel } from '.';

export interface ISourceMetricModel extends ISortableModel<number> {
  reach: number;
  earned: number;
  impression: number;
}
