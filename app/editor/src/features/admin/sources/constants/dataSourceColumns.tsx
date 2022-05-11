import { IDataSourceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core';

export const dataSourceColumns: (Column<IDataSourceModel> &
  UseSortByColumnOptions<IDataSourceModel> &
  UseFiltersColumnOptions<IDataSourceModel>)[] = [
  {
    id: 'id',
    Header: 'Source',
    accessor: 'name',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Abbreviation',
    accessor: 'code',
  },
  {
    Header: 'Type',
    accessor: (row) => row.mediaType?.name,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Last Ran On',
    accessor: 'lastRanOn',
    Cell: ({ value }: any) => <Date value={value} />,
  },
];
