import { type IFolderModel } from 'tno-core';

import { type IContentRowModel } from '@/editor/components/content';

export interface IFolderForm extends Omit<IFolderModel, 'content'> {
  content: IContentRowModel[];
}
