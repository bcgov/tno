import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';

export interface IContentState {
  initialized: boolean;
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
}
