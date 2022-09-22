import { IMediaTypeModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IMediaTypeModel> &
  UseSortByColumnOptions<IMediaTypeModel> &
  UseFiltersColumnOptions<IMediaTypeModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 7,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
