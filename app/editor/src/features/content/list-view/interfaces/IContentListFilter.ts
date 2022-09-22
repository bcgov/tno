import { ContentTypeName } from 'hooks';

import { ISortBy } from '.';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  printContent: boolean;
  contentType?: ContentTypeName;
  sourceId: number;
  otherSource: string;
  productId: number;
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  included: string;
  onTicker: string;
  commentary: string;
  topStory: string;
  sort: ISortBy[];
}
