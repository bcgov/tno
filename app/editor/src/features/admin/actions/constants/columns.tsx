import { IActionModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IActionModel> &
  UseSortByColumnOptions<IActionModel> &
  UseFiltersColumnOptions<IActionModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 300,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
