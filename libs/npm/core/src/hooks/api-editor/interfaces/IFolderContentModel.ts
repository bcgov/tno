import { IContentModel } from '.';

export interface IFolderContentModel {
  sortOrder: number;
  contentId: number;
  content?: IContentModel;
}
