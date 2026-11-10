import { type IItemModel } from '@/shared';

export interface IFileItem extends IItemModel {
  locationId: number;
  path: string;
}
