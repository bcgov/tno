import { IContentReferenceModel } from 'hooks/api-editor';
import { Page } from 'tno-core';

export const defaultContentReferencePage: Page<IContentReferenceModel> = {
  pageIndex: 0,
  pageSize: 100,
  pageCount: -1,
  items: [],
};
