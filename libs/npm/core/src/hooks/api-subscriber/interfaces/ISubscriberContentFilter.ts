import { ContentStatus, ContentTypeName, ISortPageFilter } from 'hooks/api-editor';

export interface ISubscriberContentFilter extends ISortPageFilter {
  actions?: string[];
  byline?: string;
  contentTypes: ContentTypeName[];
  createdEndOn?: string;
  createdOn?: string;
  createdStartOn?: string;
  excludeSourceIds?: number[];
  hasTopic?: boolean;
  headline?: string;
  includeHidden?: boolean;
  keyword?: string;
  names?: string;
  onlyHidden?: boolean;
  onlyPublished?: boolean;
  otherSource?: string;
  ownerId?: number;
  pageName?: string;
  productIds?: number[];
  publishedEndOn?: string;
  publishedOn?: string;
  publishedStartOn?: string;
  section?: string;
  sentiment?: number[];
  sourceIds?: number[];
  status?: ContentStatus;
  storyText?: string;
  updatedEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  userId?: number;
}
