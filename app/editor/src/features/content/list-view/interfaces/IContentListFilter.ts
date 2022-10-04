import { ContentTypeName } from 'hooks';

import { ISortBy } from '.';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  includedInCategory: boolean;
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
