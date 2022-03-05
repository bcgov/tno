import { ISortableModel } from '.';

export interface ITagModel extends ISortableModel<string> {
  description: string;
  isEnabled: boolean;
}
