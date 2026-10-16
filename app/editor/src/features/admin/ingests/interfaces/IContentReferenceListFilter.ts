import { type WorkflowStatusName } from 'tno-core';

import { type ISortBy } from '.';

export interface IContentReferenceListFilter {
  pageIndex: number;
  pageSize: number;
  /** A source code to limit to, or all the ingest's sources when empty. */
  source: string;
  uid: string;
  status: WorkflowStatusName | '';
  /** The day the references were discovered (created) by the ingest. */
  discoveredOn: Date;
  sort: ISortBy[];
}
