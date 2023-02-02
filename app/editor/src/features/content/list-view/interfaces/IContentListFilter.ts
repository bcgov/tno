import { ContentTypeName } from 'hooks';

import { ISortBy } from '.';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  includedInCategory: boolean;
  includeHidden: boolean;
  contentType?: ContentTypeName;
  sourceId: number;
  otherSource: string;
  productId: number;
  productIds?: number[];
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  showOnly?: string;
  onTicker: string;
  commentary: string;
  topStory: string;
  sort: ISortBy[];
}
