import { CellEllipsis, Checkbox, IReportModel, ITableHookColumn } from 'tno-core';

export const reportProductColumns: ITableHookColumn<IReportModel>[] = [
  {
    label: 'Subscribed',
    name: 'subscribed',
    width: 1,
    cell: (cell) => <Checkbox />,
  },
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Start time',
    name: 'start-time',
    width: 1,
    cell: () => '--',
  },
];
