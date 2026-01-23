import {
  type IContentListAdvancedFilter,
  type IContentListFilter,
} from 'features/content/interfaces';
import { type IPaged } from 'tno-core';

import { type IContentSearchResult } from './IContentSearchResult';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterPaper: IContentListFilter;
  filterPaperAdvanced: IContentListAdvancedFilter;
  searchResults?: IPaged<IContentSearchResult>;
}
