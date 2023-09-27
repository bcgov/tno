import { CellCheckbox, CellEllipsis, ITableHookColumn, ITagModel } from 'tno-core';

export const columns: ITableHookColumn<ITagModel>[] = [
  {
    label: 'Code',
    accessor: 'code',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.code}</CellEllipsis>,
  },
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Order',
    accessor: 'sortOrder',
    width: 1,
    hAlign: 'center',
    cell: (cell) => cell.original.sortOrder,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
