import { ContentTypeName } from 'tno-core';

import { ISortBy } from '.';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  hasTopic: boolean;
  includeHidden: boolean;
  onlyHidden: boolean;
  onlyPublished: boolean;
  contentTypes: ContentTypeName[];
  otherSource: string;
  productIds: number[];
  sourceIds: number[];
  excludeSourceIds: number[];
  ownerId: number | '';
  userId: number | '';
  timeFrame: number | '';
  // Actions
  showOnly?: string;
  onTicker: boolean;
  commentary: boolean;
  topStory: boolean;
  homepage: boolean;
  sort: ISortBy[];
}
