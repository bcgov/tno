import { IContentRowModel } from 'components/content';
import { IFolderModel } from 'tno-core';

export interface IFolderForm extends Omit<IFolderModel, 'content'> {
  content: IContentRowModel[];
}
