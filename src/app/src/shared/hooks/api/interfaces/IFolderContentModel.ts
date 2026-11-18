import { IContentModel } from '.';

export interface IFolderContentModel {
  sortOrder: number;
  contentId: number;
  /** The score the topic score rules give the content; an editor can choose up to it. */
  calculatedTopicScore?: number;
  content?: IContentModel;
}
