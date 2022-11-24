import { IContentReferenceModel } from 'hooks/api-editor';
import { IPage } from 'tno-core';

export const defaultContentReferencePage: IPage<IContentReferenceModel> = {
  pageIndex: 0,
  pageSize: 100,
  pageCount: -1,
  items: [],
  total: 0,
};
