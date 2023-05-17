import { CellCheckbox, CellDate, CellEllipsis, ITableHookColumn, IUserModel } from 'tno-core';

export const columns: ITableHookColumn<IUserModel>[] = [
  {
    label: 'Username',
    name: 'username',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.username}</CellEllipsis>,
  },
  {
    label: 'Email',
    name: 'email',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.email}</CellEllipsis>,
  },
  {
    label: 'Last Name',
    name: 'lastName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.lastName}</CellEllipsis>,
  },
  {
    label: 'First Name',
    name: 'firstName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.firstName}</CellEllipsis>,
  },
  {
    label: 'Role(s)',
    name: 'roles',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.roles?.join(', ')}</CellEllipsis>,
  },
  {
    label: 'Last Login',
    name: 'lastLoginOn',
    width: 2,
    cell: (cell) => <CellDate value={cell.original.lastLoginOn} />,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
  {
    label: 'Status',
    name: 'status',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.status}</CellEllipsis>,
  },
];
