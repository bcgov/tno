import { IIngestModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core';

export const columns: (Column<IIngestModel> &
  UseSortByColumnOptions<IIngestModel> &
  UseFiltersColumnOptions<IIngestModel>)[] = [
  {
    id: 'id',
    Header: 'Ingest',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Description',
    width: 3,
    accessor: 'description',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Type',
    width: 1,
    accessor: (row) => row.ingestType?.name,
    Cell: ({ value }: any) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Product',
    width: 1,
    accessor: (row) => row.product?.name,
    Cell: ({ value }: any) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Last Run',
    accessor: 'lastRanOn',
    width: 1,
    Cell: (cell) => <Date value={cell.value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
