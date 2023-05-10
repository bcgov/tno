import { CellCheckbox, CellEllipsis, IReportModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<IReportModel>[] = [
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
    label: 'Public',
    name: 'isPublic',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isPublic} />,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
