import { CellCheckbox, CellEllipsis, IFilterModel, ITableHookColumn } from 'tno-core';

export const filterColumns: ITableHookColumn<IFilterModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Owner',
    name: 'ownerId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
