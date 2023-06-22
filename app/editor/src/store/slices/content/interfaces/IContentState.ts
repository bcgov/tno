import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IPaperFilter } from 'features/content/papers/interfaces';
import { IContentModel, IPaged } from 'tno-core';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterPaper: IPaperFilter;
  filterPaperAdvanced: IContentListAdvancedFilter;
  content?: IPaged<IContentModel>;
}
