import { CellEllipsis, Checkbox, INotificationModel, ITableHookColumn, IUserModel } from 'tno-core';

export const subscriberColumns = (
  report: INotificationModel,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void,
): ITableHookColumn<IUserModel>[] => [
  {
    label: '',
    accessor: 'id',
    width: '50px',
    cell: (cell) => (
      <Checkbox
        id={`user-${cell.original.id}`}
        value={true}
        checked={report.subscribers.some((u) => u.id === cell.original.id)}
        onChange={(e) => {
          if (e.target.checked)
            setFieldValue('subscribers', [cell.original, ...report.subscribers]);
          else
            setFieldValue(
              'subscribers',
              report.subscribers.filter((s) => s.id !== cell.original.id),
            );
        }}
      />
    ),
  },
  {
    label: 'Username',
    accessor: 'username',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.username}</CellEllipsis>,
  },
  {
    label: 'Last Name',
    accessor: 'lastName',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.lastName}</CellEllipsis>,
  },
  {
    label: 'First Name',
    accessor: 'firstName',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.firstName}</CellEllipsis>,
  },
  {
    label: 'Email',
    accessor: 'email',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.email}</CellEllipsis>,
  },
];
