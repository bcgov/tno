import { Checkbox, Date, Ellipsis } from 'components/cell';
import { IDataSourceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';

export const columns: (Column<IDataSourceModel> &
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
    accessor: 'enabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Last Ran On',
    accessor: 'lastRanOn',
    Cell: ({ value }: any) => <Date value={value} />,
  },
];
