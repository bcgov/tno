import { CellEllipsis, Checkbox, IReportModel, ITableHookColumn } from 'tno-core';

export const reportProductColumns: ITableHookColumn<IReportModel>[] = [
  {
    label: 'Subscribed',
    accessor: 'subscribed',
    width: 1,
    cell: (cell) => <Checkbox />,
  },
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Start time',
    accessor: 'start-time',
    width: 1,
    cell: () => '--',
  },
];
