import { ProductTypeName } from '../constants';
import { ISortableModel, IUserModel } from '.';

export interface IProductModel extends ISortableModel<number> {
  isPublic: boolean;
  targetProductId: number;
  productType: ProductTypeName;
  subscribers: IUserModel[];
}
