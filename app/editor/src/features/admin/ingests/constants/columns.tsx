import moment from 'moment';
import { CellCheckbox, CellDate, CellEllipsis, IIngestModel, ITableHookColumn } from 'tno-core';

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
    width: 3,
    cell: (cell) => <CellDate value={cell.original.lastRanOn} format="MM/DD/YYYY HH:mm:SS A" />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];

const getStatus = (data: IIngestModel) => {
  if (data.failedAttempts >= data.retryLimit) return 'Failed';
  else if (!data.isEnabled) return 'Disabled';
  else if (!data.lastRanOn) return 'Not Running';

  const lastDelay = moment();
  const lastRanOn = moment(data.lastRanOn).add(5, 'minutes');
  return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Sleeping';
};
