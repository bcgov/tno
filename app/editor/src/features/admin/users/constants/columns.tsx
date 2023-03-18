import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellDate, CellEllipsis, IUserModel } from 'tno-core';

export const columns: (Column<IUserModel> &
  UseSortByColumnOptions<IUserModel> &
  UseFiltersColumnOptions<IUserModel>)[] = [
  {
    id: 'username',
    Header: 'Username',
    accessor: 'username',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Email',
    accessor: 'email',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Last Name',
    accessor: 'lastName',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'First Name',
    accessor: 'firstName',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Role(s)',
    accessor: 'roles',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value?.join(', ')}</CellEllipsis>,
    disableSortBy: true,
  },
  {
    Header: 'Last Login',
    accessor: 'lastLoginOn',
    width: 2,
    Cell: ({ value }: any) => <CellDate value={value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
  {
    Header: 'Status',
    accessor: 'status',
    width: 1,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
];
