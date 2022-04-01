import { IContentModel } from 'hooks/api-editor';
import { IPage } from 'tno-core/dist/components/grid-table';

export const defaultPage: IPage<IContentModel> = {
  pageIndex: 0,
  pageSize: 10,
  pageCount: -1,
  items: [],
};
