import {
  type Column,
  type UseFiltersColumnOptions,
  type UseSortByColumnOptions,
} from 'react-table';
import { CellEllipsis, type ITagModel } from 'tno-core';

/** columns for tag list popout when user is on the content form. */
export const columns: Array<
  Column<ITagModel> & UseSortByColumnOptions<ITagModel> & UseFiltersColumnOptions<ITagModel>
> = [
  {
    id: 'code',
    Header: 'Code',
    accessor: 'code',
    width: 3,
  },
  {
    id: 'name',
    Header: 'Name',
    accessor: 'name',
    width: 8,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
];
