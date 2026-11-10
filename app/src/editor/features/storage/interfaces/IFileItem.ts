import { type IItemModel } from 'tno-core';

export interface IFileItem extends IItemModel {
  locationId: number;
  path: string;
}
