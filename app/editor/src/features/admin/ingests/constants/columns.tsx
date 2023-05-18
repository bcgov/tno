import moment from 'moment';
import { CellCheckbox, CellDate, CellEllipsis, IIngestModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<IIngestModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Source',
    name: 'source.code',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.source?.code}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Type',
    name: 'ingestTypeId',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.ingestType?.name}</CellEllipsis>,
  },
  {
    label: 'Status',
    name: 'status',
    width: 1,
    cell: (cell) => {
      if (cell.original.failedAttempts >= cell.original.retryLimit) return 'Failed';
      else if (!cell.original.isEnabled) return 'Disabled';
      else if (!cell.original.lastRanOn) return 'Not Running';

      const lastDelay = moment();
      const lastRanOn = moment(cell.original.lastRanOn).add(5, 'minutes');
      return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Sleeping';
    },
  },
  {
    label: 'Last Run',
    name: 'lastRanOn',
    width: 3,
    cell: (cell) => <CellDate value={cell.original.lastRanOn} format="MM/DD/YYYY HH:mm:SS A" />,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
