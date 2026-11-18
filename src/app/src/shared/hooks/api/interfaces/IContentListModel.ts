import { ContentListActionName } from '../constants';

export interface IContentListModel {
  action: ContentListActionName;
  actionId?: number;
  actionValue?: string;
  contentIds: number[];
}
