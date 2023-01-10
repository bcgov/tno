import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportFilter } from 'features/content/morning-report/interfaces';
import { IContentModel, IPaged } from 'hooks/api-editor';

export interface IContentState {
  filter: IContentListFilter;
  filterAdvanced: IContentListAdvancedFilter;
  morningReportFilter: IMorningReportFilter;
  content?: IPaged<IContentModel>;
}
