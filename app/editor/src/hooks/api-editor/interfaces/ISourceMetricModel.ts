import { ISortableModel } from '.';

export interface ISourceMetricModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
  reach: number;
  earned: number;
  impression: number;
}
