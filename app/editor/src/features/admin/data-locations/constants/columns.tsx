import { IDataLocationModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IDataLocationModel> &
  UseSortByColumnOptions<IDataLocationModel> &
  UseFiltersColumnOptions<IDataLocationModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 5,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
