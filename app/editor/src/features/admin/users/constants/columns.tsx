import { IUserModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatUserRoles } from 'utils';

export const columns: (Column<IUserModel> &
  UseSortByColumnOptions<IUserModel> &
  UseFiltersColumnOptions<IUserModel>)[] = [
  {
    id: 'username',
    Header: 'Username',
    accessor: 'username',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Email',
    accessor: 'email',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Last Name',
    accessor: 'lastName',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'First Name',
    accessor: 'firstName',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Role(s)',
    accessor: 'roles',
    width: 2,
    Cell: ({ value }) => <Ellipsis>{formatUserRoles(value)}</Ellipsis>,
    disableSortBy: true,
  },
  {
    Header: 'Last Login',
    accessor: 'lastLoginOn',
    width: 2,
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Status',
    accessor: 'status',
    width: 1,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
];
