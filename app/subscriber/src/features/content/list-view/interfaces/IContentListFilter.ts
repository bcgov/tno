import { ContentStatus, ContentTypeName } from 'tno-core';

import { ISortBy } from './ISortBy';

export interface IContentListFilter {
  byline?: string;
  contentTypes: ContentTypeName[];
  excludeSourceIds?: number[];
  headline?: string;
  keyword?: string;
  otherSource?: string | '';
  ownerId?: number | '';
  pageIndex: number;
  pageSize: number;
  productIds?: number[];
  showOnly?: string;
  sort: ISortBy[];
  sourceIds?: number[];
  status?: ContentStatus;
  storyText?: string;
  userId?: number | '';
}
