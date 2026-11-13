import { ISortableModel, IUserProductModel } from '.';
import { ProductTypeName } from '../constants';

export interface IProductModel extends ISortableModel<number> {
  isPublic: boolean;
  targetProductId: number;
  productType: ProductTypeName;
  subscribers: IUserProductModel[];
}
