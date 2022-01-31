import { IContentModel } from '.';

export interface IPrintContentModel {
  contentId: number;
  content: IContentModel;
  edition: string;
  section: string;
  storyType: string;
  byline: string;
}
