import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IPaperFilter } from 'features/content/papers/interfaces';
import { IPaged } from 'tno-core';

import { IContentSearchResult } from './IContentSearchResult';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterPaper: IPaperFilter;
  filterPaperAdvanced: IContentListAdvancedFilter;
  searchResults?: IPaged<IContentSearchResult>;
}
