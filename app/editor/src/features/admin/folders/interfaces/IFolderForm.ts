import { type IContentRowModel } from 'components/content';
import { type IFolderModel } from 'tno-core';

export interface IFolderForm extends Omit<IFolderModel, 'content'> {
  content: IContentRowModel[];
}
