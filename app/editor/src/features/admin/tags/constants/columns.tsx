import { ITagModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<ITagModel> &
  UseSortByColumnOptions<ITagModel> &
  UseFiltersColumnOptions<ITagModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    width: 300,
    accessor: 'name',
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
