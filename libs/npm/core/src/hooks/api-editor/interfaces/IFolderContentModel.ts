import { IContentModel } from '.';

export interface IFolderContentModel extends IContentModel {
  sortOrder: number;
  contentId: number;
}
