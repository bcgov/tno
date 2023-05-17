import { CellCheckbox, CellEllipsis, ILicenseModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<ILicenseModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 7,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'TTL',
    name: 'ttl',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.ttl}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
