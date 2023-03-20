import { ContentListActionName } from '../constants';

export interface IContentListModel {
  action: ContentListActionName;
  actionName?: string;
  actionValue?: string;
  contentIds: number[];
}
