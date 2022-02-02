import { ContentStatus, LogicalOperator } from '..';

export interface IContentFilter {
  mediaTypeId?: number;
  contentTypeId?: number;
  source?: string;
  sourceId?: number;
  ownerId?: number;
  page?: string;
  section?: string;
  status?: ContentStatus;
  createdOn?: Date;
  createdStartOn?: Date;
  createdEndOn?: Date;
  updatedOn?: Date;
  updatedStartOn?: Date;
  updatedEndOn?: Date;
  publishedOn?: Date;
  publishedStartOn?: Date;
  publishedEndOn?: Date;
  actions?: string[];
  hasPage?: boolean;
  logicalOperator?: LogicalOperator;
}
