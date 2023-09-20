import { ContentStatus, ContentTypeName, LogicalOperator } from '..';
import { ISortPageFilter } from '.';

export interface IContentFilter extends ISortPageFilter {
  otherSource?: string;
  ownerId?: number;
  userId?: number;
  contentIds?: number[];
  pageName?: string;
  section?: string;
  status?: ContentStatus;
  hasTopic?: boolean;
  includeHidden?: boolean;
  onlyHidden?: boolean;
  onlyPublished?: boolean;
  createdOn?: string;
  createdStartOn?: string;
  createdEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  updatedEndOn?: string;
  publishedOn?: string;
  publishedStartOn?: string;
  publishedEndOn?: string;
  contentTypes: ContentTypeName[];
  keyword?: string;
  actions?: string[];
  productIds?: number[];
  sourceIds?: number[];
  seriesId?: number;
  excludeSourceIds?: number[];
  logicalOperator?: LogicalOperator;
}
