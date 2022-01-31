import { ContentStatus, LogicalOperator } from '..';

export interface IContentFilter {
  mediaTypeId?: number;
  contentTypeId?: number;
  source?: string;
  dataSourceId?: number;
  ownerId?: number;
  userId?: number;
  pageName?: string;
  section?: string;
  status?: ContentStatus;
  createdOn?: string;
  createdStartOn?: string;
  createdEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  updatedEndOn?: string;
  publishedOn?: string;
  publishedStartOn?: string;
  publishedEndOn?: string;
  actions?: string[];
  logicalOperator?: LogicalOperator;
  sort?: string[];
}
