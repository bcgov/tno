import {
  IContentListAdvancedFilter,
  IContentListFilter,
  ISortBy,
} from 'features/content/list-view/interfaces';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  sortBy: ISortBy[];
}
