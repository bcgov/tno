import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportFilter } from 'features/content/morning-report/interfaces';
import { IContentModel, IPaged } from 'tno-core';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  filterMorningReport: IMorningReportFilter;
  content?: IPaged<IContentModel>;
}
