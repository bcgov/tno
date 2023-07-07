import { CellDate, CellEllipsis, IReportInstanceModel, ITableHookColumn } from 'tno-core';

export const instanceColumns = (): ITableHookColumn<IReportInstanceModel>[] => [
  {
    label: 'Published On',
    name: 'publishedOn',
    width: 1,
    cell: (cell) => <CellDate value={cell.original.publishedOn} />,
  },
  {
    label: 'Owner',
    name: 'owner.username',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username ?? ''}</CellEllipsis>,
  },
  {
    label: 'Transaction Id',
    name: 'txId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.response.txId}</CellEllipsis>,
  },
];
