import { WorkflowStatusName } from 'tno-core';

import { ISortBy } from '.';

export interface IContentReferenceListFilter {
  pageIndex: number;
  pageSize: number;
  sources: string[];
  mediaTypeIds: number[];
  uid: string;
  topic: string;
  status: WorkflowStatusName | '';
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
