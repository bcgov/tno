import { ContentStatus, ContentTypeName, ISortPageFilter, LogicalOperator } from 'tno-core';

export interface IContentFilter extends ISortPageFilter {
  otherSource?: string;
  ownerId?: number;
  userId?: number;
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
  keyword?: string;
  publishedOn?: string;
  publishedStartOn?: string;
  publishedEndOn?: string;
  contentTypes: ContentTypeName[];
  actions?: string[];
  productIds?: number[];
  sourceIds?: number[];
  logicalOperator?: LogicalOperator;
}
