import { IItemModel } from '.';

export interface IFolderModel {
  path: string;
  items: IItemModel[];
  isLocal: boolean;
}
