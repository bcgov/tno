import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import { IPaged } from 'tno-core';

import { IContentSearchResult } from './IContentSearchResult';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterPaper: IContentListFilter;
  filterPaperAdvanced: IContentListAdvancedFilter;
  searchResults?: IPaged<IContentSearchResult>;
}
