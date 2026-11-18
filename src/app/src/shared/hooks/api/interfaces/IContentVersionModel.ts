import { IAuditColumnsModel } from '..';

export interface IContentVersionModel extends IAuditColumnsModel {
  ownerId?: number;
  headline?: string;
  byline?: string;
  edition?: string;
  section?: string;
  page?: string;
  summary?: string;
  body?: string;
}
