import { IContentReferenceListFilter } from '../interfaces';

export const defaultContentReferenceFilter: IContentReferenceListFilter = {
  pageIndex: 0,
  pageSize: 100,
  sources: [],
  mediaTypeIds: [],
  uid: '',
  topic: '',
  status: '',
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
