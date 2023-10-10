import { IContentModel } from '../../../hooks/api-editor';

export interface IContentRowModel {
  sortOrder: number;
  content: IContentModel;
  selected: boolean;
}
