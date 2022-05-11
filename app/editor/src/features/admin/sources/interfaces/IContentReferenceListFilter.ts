import { WorkflowStatusName } from 'hooks/api-editor';

import { ISortBy } from '.';

export interface IContentReferenceListFilter {
  pageIndex: number;
  pageSize: number;
  source: string;
  uid: string;
  topic: string;
  workflowStatus: WorkflowStatusName | '';
  offset: number | '';
  partition: number | '';
  publishedOn: string;
  publishedStartOn: string;
  publishedEndOn: string;
  updatedOn: string;
  updatedStartOn: string;
  updatedEndOn: string;
  sort: ISortBy[];
}
