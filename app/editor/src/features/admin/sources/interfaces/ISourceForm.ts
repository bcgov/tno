import { ISourceModel } from 'tno-core';

export interface ISourceForm
  extends Omit<ISourceModel, 'productId' | 'ownerId' | 'productSearchGroupId'> {
  productId: number | '';
  productSearchGroupId: number | '';
  ownerId: number | '';
}
