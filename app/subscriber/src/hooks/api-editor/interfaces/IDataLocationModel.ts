import { IConnectionModel, ISortableModel } from '.';

export interface IDataLocationModel extends ISortableModel<number> {
  connectionId?: number;
  connection?: IConnectionModel;
}
