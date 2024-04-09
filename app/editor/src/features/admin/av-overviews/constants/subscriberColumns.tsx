import { CellEllipsis, Checkbox, Col, ITableHookColumn, IUserAVOverviewModel } from 'tno-core';

import { IAVOverviewTemplateForm } from '../interfaces';

export const subscriberColumns = (
  overviewForm: IAVOverviewTemplateForm,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void,
): ITableHookColumn<IUserAVOverviewModel>[] => [
  {
    label: '',
    accessor: 'id',
    width: '50px',
    cell: (cell) => (
      <Checkbox
        id={`user-${cell.original.id}`}
        value={true}
        checked={overviewForm.subscribers.some((u) => u.id === cell.original.id && u.isSubscribed)}
        onChange={(e) => {
          const user = { ...cell.original, isSubscribed: e.target.checked };
          if (overviewForm.subscribers.some((u) => u.id === cell.original.id))
            setFieldValue(
              'subscribers',
              overviewForm.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
            );
          else setFieldValue('subscribers', [user, ...overviewForm.subscribers]);
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
      <Col>
        <CellEllipsis>{cell.original.email}</CellEllipsis>
        {cell.original.preferredEmail && (
          <CellEllipsis className="preferred">{cell.original.preferredEmail}</CellEllipsis>
        )}
      </Col>
    ),
  },
];
