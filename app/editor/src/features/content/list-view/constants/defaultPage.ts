import { IPage } from 'components/grid-table';
import { IContentModel } from 'hooks/api-editor';

export const defaultPage: IPage<IContentModel> = {
  pageIndex: 0,
  pageSize: 10,
  pageCount: -1,
  items: [],
};
