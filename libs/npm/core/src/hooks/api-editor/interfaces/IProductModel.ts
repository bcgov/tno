import { ProductTypeName } from '../constants';
import { ISortableModel, IUserModel } from '.';

export interface IProductModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  targetProductId: number;
  productType: ProductTypeName;
  subscribers: IUserModel[];
}
