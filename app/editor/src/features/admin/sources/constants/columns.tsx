import { CellCheckbox, CellEllipsis, ISourceModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<ISourceModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Code',
    name: 'code',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.code}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 7,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Topics',
    name: 'useInTopics',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.useInTopics} />,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
