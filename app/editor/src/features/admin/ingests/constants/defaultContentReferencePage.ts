import { IContentReferenceModel, IPage } from 'tno-core';

export const defaultContentReferencePage: IPage<IContentReferenceModel> = {
  pageIndex: 0,
  pageSize: 50,
  pageCount: -1,
  items: [],
  total: 0,
};
