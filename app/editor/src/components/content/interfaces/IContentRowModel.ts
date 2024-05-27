import { IContentModel } from 'tno-core';

export interface IContentRowModel {
  sortOrder: number;
  content: IContentModel;
  selected: boolean;
}
