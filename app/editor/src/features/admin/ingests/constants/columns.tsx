import { CellCheckbox, CellDate, CellEllipsis, IIngestModel, ITableHookColumn } from 'tno-core';

import { getStatus } from '../utils';

export const columns: ITableHookColumn<IIngestModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Source',
    accessor: 'source.code',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.source?.code}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Type',
    accessor: 'ingestTypeId',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.ingestType?.name}</CellEllipsis>,
  },
  {
    label: 'Status',
    accessor: (data: IIngestModel) => getStatus(data),
    width: 1,
  },
  {
    label: 'Last Run',
    accessor: 'lastRanOn',
    width: '200px',
    cell: (cell) => <CellDate value={cell.original.lastRanOn} format="MM/DD/YYYY HH:mm:SS A" />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: '100px',
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
