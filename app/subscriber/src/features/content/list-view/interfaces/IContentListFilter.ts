import { ContentStatus, ContentTypeName } from 'tno-core';

import { ISortBy } from './ISortBy';

export interface IContentListFilter {
  actions?: string[];
  boldKeywords?: boolean;
  byline?: string;
  contentTypes: ContentTypeName[];
  excludeSourceIds?: number[];
  hasFile?: boolean;
  headline?: string;
  keyword?: string;
  names?: string;
  otherSource?: string | '';
  ownerId?: number | '';
  pageIndex: number;
  pageSize: number;
  productIds?: number[];
  showOnly?: string;
  sort: ISortBy[];
  sourceIds?: number[];
  status?: ContentStatus;
  sentiment?: number[];
  storyText?: string;
  topStory?: boolean;
  userId?: number | '';
  useUnpublished?: boolean;
}
