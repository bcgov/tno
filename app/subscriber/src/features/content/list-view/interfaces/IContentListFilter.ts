import { ContentStatus, ContentTypeName } from 'tno-core';

import { ISortBy } from './ISortBy';

export interface IContentListFilter {
  actions?: string[];
  boldKeywords?: boolean;
  contentTypes: ContentTypeName[];
  excludeSourceIds?: number[];
  hasFile?: boolean;
  keyword?: string;
  names?: string;
  otherSource?: string | '';
  ownerId?: number | '';
  pageIndex: number;
  pageSize: number;
  mediaTypeIds?: number[];
  showOnly?: string;
  sort: ISortBy[];
  sourceIds?: number[];
  status?: ContentStatus;
  sentiment?: number[];
  searchTerm?: string;
  topStory?: boolean;
  userId?: number | '';
  useUnpublished?: boolean; // TODO: Rename 'searchUnpublished' to be consistent with filter settings.
  inHeadline?: boolean;
  inByline?: boolean;
  inStory?: boolean;
}
