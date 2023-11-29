import {
  CellEllipsis,
  Checkbox,
  INotificationModel,
  ITableHookColumn,
  IUserModel,
  Show,
  ToggleGroup,
} from 'tno-core';

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
        checked={report.subscribers.some((u) => u.id === cell.original.id && u.isSubscribed)}
        disabled={report.subscribers.some(
          // disable the checkbox if a user has requested a change in their
          // subscription status but an admin has not yet approved it
          (u) =>
            u.id === cell.original.id &&
            u.requestedIsSubscribedStatus !== undefined &&
            u.subscriptionChangeActioned !== undefined,
        )}
        onChange={(e) => {
          const user = {
            ...cell.original,
            isSubscribed: e.target.checked,
            subscriptionChangeActioned: true,
          };
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
    cell: (cell) => <CellEllipsis>{cell.original.email}</CellEllipsis>,
  },
  {
    label: 'Change Approval',
    accessor: 'change-approved',
    width: '1.5',
    showSort: false,
    cell: (cell) => (
      <Show
        visible={report.subscribers.some(
          (u) =>
            u.id === cell.original.id &&
            u.requestedIsSubscribedStatus !== undefined &&
            u.subscriptionChangeActioned !== undefined,
        )}
      >
        <Checkbox
          id={`user-${cell.original.id}-target-status`}
          value={true}
          checked={report.subscribers.some(
            (u) => u.id === cell.original.id && u.requestedIsSubscribedStatus,
          )}
          disabled={true}
          title="Target Subscription Status"
        />
        <ToggleGroup
          className="approval-actions"
          options={[
            {
              label: 'APPROVE',
              onClick: () => {
                const targetStatus = report.subscribers.filter((u) => u.id === cell.original.id)[0]
                  .requestedIsSubscribedStatus;
                const user = {
                  ...cell.original,
                  isSubscribed: targetStatus,
                  requestedIsSubscribedStatus: targetStatus,
                  subscriptionChangeActioned: true,
                };
                if (report.subscribers.some((u) => u.id === cell.original.id)) {
                  setFieldValue(
                    'subscribers',
                    report.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
                  );
                }
              },
            },
            {
              label: 'REJECT',
              onClick: () => {
                const targetStatus = report.subscribers.filter((u) => u.id === cell.original.id)[0]
                  .requestedIsSubscribedStatus;
                const user = {
                  ...cell.original,
                  isSubscribed: !targetStatus,
                  requestedIsSubscribedStatus: targetStatus,
                  subscriptionChangeActioned: true,
                };
                if (report.subscribers.some((u) => u.id === cell.original.id)) {
                  setFieldValue(
                    'subscribers',
                    report.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
                  );
                }
              },
            },
          ]}
        />
      </Show>
    ),
  },
];
