import { IContentModel } from '.';

export interface IFolderContentModel {
  sortOrder: number;
  contentId: number;
  maxTopicScore?: number;
  content?: IContentModel;
}
