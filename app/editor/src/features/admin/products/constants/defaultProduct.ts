import { IProductModel, ProductTypeName } from 'tno-core';

export const defaultProduct: IProductModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  productType: ProductTypeName.Report,
  targetProductId: 0,
  subscribers: [],
  isPublic: false,
};
