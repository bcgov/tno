import {
  ContentStatusName,
  ContentTypeName,
  IFilterActionSettingsModel,
  ISortField,
} from 'tno-core';

export interface IContentListFilter {
  actions?: IFilterActionSettingsModel[];
  boldKeywords?: boolean;
  commentary?: boolean;
  contentTypes: ContentTypeName[];
  contentIds?: number[];
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
  ownerId?: number;
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
  sort: ISortField[];
  sourceIds?: number[];
  status?: ContentStatusName;
  tags?: string[];
  topStory?: boolean;
  userId?: number;
}
