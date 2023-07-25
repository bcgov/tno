import { ContentStatus, ContentTypeName } from 'tno-core';

import { ISortBy } from '.';

export interface IContentListFilter {
  byline?: string;
  contentTypes?: ContentTypeName[];
  excludeSourceIds?: number[];
  headline?: string;
  keyword?: string;
  otherSource?: string | '';
  ownerId?: number | '';
  productIds?: number[];
  showOnly?: string;
  sourceIds?: number[];
  status?: ContentStatus;
  storyText?: string;
  userId?: number | '';
}
