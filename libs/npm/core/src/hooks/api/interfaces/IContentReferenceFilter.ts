import { WorkflowStatusName } from '../constants';
import { ISortPageFilter } from '.';

export interface IContentReferenceFilter extends ISortPageFilter {
  sources?: string[];
  mediaTypeIds?: number[];
  uid?: string;
  topic?: string;
  status?: WorkflowStatusName;
  publishedOn?: string;
  publishedStartOn?: string;
  publishedEndOn?: string;
  updatedOn?: string;
  updatedStartOn?: string;
  updatedEndOn?: string;
}
