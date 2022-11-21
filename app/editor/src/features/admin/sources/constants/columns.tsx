import { ISourceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core';

export const columns: (Column<ISourceModel> &
  UseSortByColumnOptions<ISourceModel> &
  UseFiltersColumnOptions<ISourceModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    sortType: 'string',
    width: 3,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Code',
    accessor: 'code',
    sortType: 'string',
    width: 1,
  },
  {
    Header: 'Licence',
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
