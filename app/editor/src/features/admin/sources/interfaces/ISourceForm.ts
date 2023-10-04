import { ISourceModel } from 'tno-core';

export interface ISourceForm extends Omit<ISourceModel, 'productId' | 'ownerId'> {
  productId: number | '';
  ownerId: number | '';
}
