import { ContentStatus, ContentTypeName } from 'tno-core';

import { ISortBy } from './ISortBy';

export interface IContentListFilter {
  actions?: string[];
  boldKeywords?: boolean;
  contentTypes: ContentTypeName[];
  contributorIds?: number[];
  dateOffset?: number;
  edition?: string;
  excludeSourceIds?: number[];
  frontPage?: boolean;
  hasFile?: boolean;
  inByline?: boolean;
  inHeadline?: boolean;
  inStory?: boolean;
  keyword?: string;
  mediaTypeIds?: number[];
  names?: string;
  otherSource?: string | '';
  ownerId?: number | '';
  page?: string;
  pageIndex: number;
  pageSize: number;
  publishedEndOn?: string;
  publishedStartOn?: string;
  savedSearchId?: number;
  searchTerm?: string;
  searchUnpublished?: boolean;
  section?: string;
  sentiment?: number[];
  seriesIds?: number[];
  showOnly?: string;
  sort: ISortBy[];
  sourceIds?: number[];
  status?: ContentStatus;
  tags?: string[];
  topStory?: boolean;
  userId?: number | '';
}
