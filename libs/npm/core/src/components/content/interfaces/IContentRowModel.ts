import { IContentModel } from '../../../hooks/api';
export interface IContentRowModel {
  sortOrder: number;
  content: IContentModel;
  selected: boolean;
}
