import { IMediaTypeModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IMediaTypeModel> &
  UseSortByColumnOptions<IMediaTypeModel> &
  UseFiltersColumnOptions<IMediaTypeModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
  },
  {
    Header: 'Updated By',
    accessor: 'updatedBy',
  },
  {
    Header: 'Created By',
    accessor: 'createdBy',
  },
  {
    Header: 'Created On',
    accessor: 'createdOn',
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
];
