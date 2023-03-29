import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportsFilter } from 'features/content/morning-reports/interfaces';
import { IContentModel, IPaged } from 'tno-core';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterMorningReports: IMorningReportsFilter;
  content?: IPaged<IContentModel>;
}
