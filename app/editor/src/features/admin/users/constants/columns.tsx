import { IUserModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IUserModel> &
  UseSortByColumnOptions<IUserModel> &
  UseFiltersColumnOptions<IUserModel>)[] = [
  {
    id: 'id',
    Header: 'Username',
    accessor: 'username',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Email',
    accessor: 'email',
  },
  {
    Header: 'Last Name',
    accessor: 'lastName',
  },
  {
    Header: 'First Name',
    accessor: 'firstName',
  },
  {
    Header: 'Last Login',
    accessor: 'lastLoginOn',
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Status',
    accessor: 'status',
  },
];
