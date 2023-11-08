import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentModel, IOptionItem, IPaged } from 'tno-core';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  content?: IPaged<IContentModel>;
  dateFilter?: IOptionItem | null;
  pressFilter?: IOptionItem | null;
}
