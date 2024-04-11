import {
  CellEllipsis,
  Checkbox,
  INotificationModel,
  ITableHookColumn,
  IUserNotificationModel,
  Row,
} from 'tno-core';

export const subscriberColumns = (
  report: INotificationModel,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void,
): ITableHookColumn<IUserNotificationModel>[] => [
  {
    label: '',
    accessor: 'id',
    width: '50px',
    cell: (cell) => (
      <Checkbox
        id={`user-${cell.original.id}`}
        value={true}
        checked={report.subscribers.some((u) => u.id === cell.original.id && u.isSubscribed)}
        onChange={(e) => {
          const user = { ...cell.original, isSubscribed: e.target.checked };
          if (report.subscribers.some((u) => u.id === cell.original.id))
            setFieldValue(
              'subscribers',
              report.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
            );
          else setFieldValue('subscribers', [user, ...report.subscribers]);
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
    cell: (cell) => (
      <Row gap="0.15rem">
        <CellEllipsis>{cell.original.email}</CellEllipsis>
        {cell.original.preferredEmail && (
          <CellEllipsis className="preferred">{cell.original.preferredEmail}</CellEllipsis>
        )}
      </Row>
    ),
  },
];
