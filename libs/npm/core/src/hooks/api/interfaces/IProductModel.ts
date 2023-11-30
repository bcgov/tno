import { ProductTypeName } from '../constants';
import { ISortableModel, IUserSubscriberModel } from '.';

export interface IProductModel extends ISortableModel<number> {
  isPublic: boolean;
  targetProductId: number;
  productType: ProductTypeName;
  subscribers: IUserSubscriberModel[];
}
