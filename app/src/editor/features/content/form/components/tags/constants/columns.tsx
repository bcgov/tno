import { CellEllipsis, type ITableHookColumn, type ITagModel } from '@/shared';

/** columns for tag list popout when user is on the content form. */
export const columns: Array<ITableHookColumn<ITagModel>> = [
  {
    label: 'Code',
    accessor: 'code',
    sort: 'code',
    width: 3,
    cell: (cell) => <>{cell.original.code}</>,
  },
  {
    label: 'Name',
    accessor: 'name',
    sort: 'name',
    width: 8,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
];
