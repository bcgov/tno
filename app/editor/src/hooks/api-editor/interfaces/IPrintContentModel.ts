import { IAuditColumnsModel, IContentModel } from '.';

export interface IPrintContentModel extends IAuditColumnsModel {
  contentId: number;
  content?: IContentModel;
  edition: string;
  section: string;
  storyType: string;
  byline: string;
}
