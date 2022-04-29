import { DataLocationType } from '..';
import { ISortableModel } from '.';

export interface IDataLocationModel extends ISortableModel<number> {
  locationType: DataLocationType;
  connection?: any;
}
