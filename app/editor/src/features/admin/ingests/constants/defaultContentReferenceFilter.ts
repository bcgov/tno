import { type IContentReferenceListFilter } from '../interfaces';

export const defaultContentReferenceFilter: IContentReferenceListFilter = {
  pageIndex: 0,
  pageSize: 100,
  source: '',
  uid: '',
  status: '',
  discoveredOn: new Date(),
  sort: [{ id: 'createdOn', desc: true }],
};
