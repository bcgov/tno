import { type IContentModel } from '@/shared';

export interface IContentRowModel {
  sortOrder: number;
  content: IContentModel;
  selected: boolean;
}
