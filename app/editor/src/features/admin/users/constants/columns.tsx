import { CellCheckbox, CellDate, CellEllipsis, ITableHookColumn, IUserModel } from 'tno-core';

export const columns: ITableHookColumn<IUserModel>[] = [
  {
    label: 'Username',
    accessor: 'username',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.username}</CellEllipsis>,
  },
  {
    label: 'Email',
    accessor: 'email',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.email}</CellEllipsis>,
  },
  {
    label: 'Last Name',
    accessor: 'lastName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.lastName}</CellEllipsis>,
  },
  {
    label: 'First Name',
    accessor: 'firstName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.firstName}</CellEllipsis>,
  },
  {
    label: 'Role(s)',
    accessor: 'roles',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.roles?.join(', ')}</CellEllipsis>,
  },
  {
    label: 'Last Login',
    accessor: 'lastLoginOn',
    width: 2,
    cell: (cell) => <CellDate value={cell.original.lastLoginOn} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
  {
    label: 'Status',
    accessor: 'status',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.status}</CellEllipsis>,
  },
];
