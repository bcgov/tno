import { ISortBy } from 'features/interfaces';
import { ContentTypeName } from 'tno-core';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  onlyPublished: boolean;
  hasTopic: boolean;
  isHidden: boolean | '';
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  otherSource: string;
  contentTypes: ContentTypeName[];
  productIds: number[];
  sourceIds: number[];
  excludeSourceIds: number[];
  // Actions
  showOnly?: string;
  onTicker: boolean;
  commentary: boolean;
  topStory: boolean;
  homepage: boolean;
  sort: ISortBy[];
}
