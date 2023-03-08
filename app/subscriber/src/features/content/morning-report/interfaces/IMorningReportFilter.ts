import { ContentTypeName } from 'hooks';

import { IContentListAdvancedFilter, ISortBy } from '../../list-view/interfaces';

export interface IMorningReportFilter extends IContentListAdvancedFilter {
  pageIndex: number;
  pageSize: number;
  includedInCategory: boolean;
  includeHidden: boolean;
  contentType?: ContentTypeName;
  sourceId: number;
  otherSource: string;
  productIds: number[];
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  onTicker: string;
  commentary: string;
  topStory: string;
  sort: ISortBy[];
}
