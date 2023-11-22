import { IProductModel, ProductType, ProductTypeName } from 'tno-core';

export const defaultProduct: IProductModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  targetProductId: 0,
  productType: ProductTypeName.Report,
  subscribers: [],
};
