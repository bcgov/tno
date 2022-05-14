import { IContentReferenceListFilter } from '../interfaces';

export const defaultContentReferenceFilter: IContentReferenceListFilter = {
  pageIndex: 0,
  pageSize: 100,
  source: '',
  uid: '',
  topic: '',
  workflowStatus: '',
  offset: 0,
  partition: 0,
  publishedOn: '',
  publishedStartOn: '',
  publishedEndOn: '',
  updatedOn: '',
  updatedStartOn: '',
  updatedEndOn: '',
  sort: [],
};
