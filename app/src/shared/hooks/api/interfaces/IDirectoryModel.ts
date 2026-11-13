import { IItemModel } from '.';

export interface IDirectoryModel {
  path: string;
  items: IItemModel[];
  isLocal: boolean;
}
