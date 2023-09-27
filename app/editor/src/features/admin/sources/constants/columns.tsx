import { CellCheckbox, CellEllipsis, ISourceModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<ISourceModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Code',
    accessor: 'code',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.code}</CellEllipsis>,
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
    label: 'Topics',
    accessor: 'useInTopics',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.useInTopics} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
