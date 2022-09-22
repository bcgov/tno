import { ISourceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core';

export const columns: (Column<ISourceModel> &
  UseSortByColumnOptions<ISourceModel> &
  UseFiltersColumnOptions<ISourceModel>)[] = [
  {
    id: 'id',
    Header: 'Source',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Code',
    accessor: 'code',
    width: 1,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 4,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'License',
    width: 1,
    accessor: (row) => row.license?.name,
    Cell: ({ value }: any) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
