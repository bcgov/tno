import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentModel, IPaged } from 'tno-core';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  content?: IPaged<IContentModel>;
}
