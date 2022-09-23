import { ITagModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<ITagModel> &
  UseSortByColumnOptions<ITagModel> &
  UseFiltersColumnOptions<ITagModel>)[] = [
  {
    id: 'id',
    Header: 'Code',
    accessor: 'id',
    width: 1,
  },
  {
    id: 'name',
    Header: 'Name',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'description',
    Header: 'Description',
    width: 5,
    accessor: 'description',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'isEnabled',
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
