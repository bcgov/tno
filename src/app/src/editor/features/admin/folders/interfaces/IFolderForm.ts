import { type IContentRowModel } from '@/editor/components/content';
import { type IFolderModel } from '@/shared';

export interface IFolderForm extends Omit<IFolderModel, 'content'> {
  content: IContentRowModel[];
}
