import { ContentTypeName } from 'hooks';

import { ISortBy } from '../../list-view/interfaces';

export interface IMorningReportFilter {
  pageIndex: number;
  pageSize: number;
  includedInCategory: boolean;
  includeHidden: boolean;
  contentType?: ContentTypeName;
  sourceId: number;
  otherSource: string;
  productId: number;
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  onTicker: string;
  commentary: string;
  topStory: string;
  sort: ISortBy[];
}
