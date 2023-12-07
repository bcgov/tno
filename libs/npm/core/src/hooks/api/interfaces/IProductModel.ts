import { ProductTypeName } from '../constants';
import { ISortableModel, IUserProductModel } from '.';

export interface IProductModel extends ISortableModel<number> {
  isPublic: boolean;
  targetProductId: number;
  productType: ProductTypeName;
  subscribers: IUserProductModel[];
}
