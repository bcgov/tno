import { ISortableModel } from '.';

export interface ISeriesModel extends ISortableModel<number> {
  description: string;
  isEnabled: boolean;
}
