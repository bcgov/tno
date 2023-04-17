import { ContentTypeName } from 'tno-core';

import { ISortBy } from '.';

export interface IContentListFilter {
  pageIndex: number;
  pageSize: number;
  hasTopic: boolean;
  includeHidden: boolean;
  contentTypes: ContentTypeName[];
  sourceId: number;
  otherSource: string;
  productIds: number[];
  sourceIds: number[];
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
