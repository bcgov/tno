import { IIngestModel } from 'hooks/api-editor';
import moment from 'moment';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellDate, CellEllipsis } from 'tno-core';

export const columns: (Column<IIngestModel> &
  UseSortByColumnOptions<IIngestModel> &
  UseFiltersColumnOptions<IIngestModel>)[] = [
  {
    id: 'id',
    Header: 'Ingest',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Type',
    width: 1,
    accessor: (row) => row.ingestType?.name,
    Cell: ({ value }: any) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Product',
    width: 1,
    accessor: (row) => row.product?.name,
    Cell: ({ value }: any) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Status',
    width: 1,
    accessor: (row) => {
      if (row.failedAttempts >= row.retryLimit) return 'Failed';
      else if (!row.isEnabled) return 'Disabled';
      else if (!row.lastRanOn) return 'Not Running';

      const lastDelay = moment();
      const lastRanOn = moment(row.lastRanOn).add(5, 'minutes');
      return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Sleeping';
    },
  },
  {
    Header: 'Last Run',
    accessor: 'lastRanOn',
    width: 2,
    Cell: (cell) => <CellDate value={cell.value} format="MM/DD/YYYY HH:mm:SS A" />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    sortType: 'basic',
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
];
