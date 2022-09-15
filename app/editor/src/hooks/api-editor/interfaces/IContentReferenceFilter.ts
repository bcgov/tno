import { WorkflowStatusName } from '../constants';
import { IPageFilter } from '.';

export interface IContentReferenceFilter extends IPageFilter {
  source?: string;
  uid?: string;
  topic?: string;
  status?: WorkflowStatusName;
  offset?: number;
  partition?: number;
  publishedOn?: string;
  publishedStartOn?: string;
  publishedEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  updatedEndOn?: string;
  sort?: string[];
}
