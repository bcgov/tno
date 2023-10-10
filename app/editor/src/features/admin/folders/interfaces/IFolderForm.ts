import { IContentRowModel, IFolderModel } from 'tno-core';

export interface IFolderForm extends Omit<IFolderModel, 'content'> {
  content: IContentRowModel[];
}
