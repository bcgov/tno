import { IItemModel } from 'hooks';

export interface IFileItem extends IItemModel {
  locationId: number;
  path: string;
}
