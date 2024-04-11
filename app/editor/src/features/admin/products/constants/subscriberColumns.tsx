import {
  CellEllipsis,
  Checkbox,
  IProductModel,
  ITableHookColumn,
  IUserProductModel,
  Row,
  Show,
  ToggleGroup,
} from 'tno-core';

export const subscriberColumns = (
  product: IProductModel,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void,
): ITableHookColumn<IUserProductModel>[] => [
  {
    label: '',
    accessor: 'id',
    width: '50px',
    cell: (cell) => (
      <Checkbox
        id={`user-${cell.original.id}`}
        value={true}
        checked={product.subscribers.some((u) => u.id === cell.original.id && u.isSubscribed)}
        disabled={product.subscribers.some(
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
          if (product.subscribers.some((u) => u.id === cell.original.id))
            setFieldValue(
              'subscribers',
              product.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
            );
          else setFieldValue('subscribers', [user, ...product.subscribers]);
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
  {
    label: 'Change Approval',
    accessor: 'change-approved',
    width: '1.5',
    showSort: false,
    cell: (cell) => (
      <Show
        visible={product.subscribers.some(
          (u) =>
            u.id === cell.original.id &&
            u.requestedIsSubscribedStatus !== undefined &&
            u.subscriptionChangeActioned !== undefined,
        )}
      >
        <Checkbox
          id={`user-${cell.original.id}-target-status`}
          value={true}
          defaultChecked={product.subscribers.some(
            (u) => u.id === cell.original.id && u.requestedIsSubscribedStatus,
          )}
          disabled={true}
          title={
            product.subscribers.filter((u) => u.id === cell.original.id)[0]
              ?.requestedIsSubscribedStatus
              ? 'Request is to SUBSCRIBE'
              : 'Request is to UNSUBSCRIBE'
          }
        />
        <ToggleGroup
          className="approval-actions"
          options={[
            {
              label: 'APPROVE',
              onClick: () => {
                const targetStatus = product.subscribers.filter((u) => u.id === cell.original.id)[0]
                  .requestedIsSubscribedStatus;
                const user = {
                  ...cell.original,
                  isSubscribed: targetStatus,
                  requestedIsSubscribedStatus: targetStatus,
                  subscriptionChangeActioned: true,
                };
                if (product.subscribers.some((u) => u.id === cell.original.id)) {
                  setFieldValue(
                    'subscribers',
                    product.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
                  );
                }
              },
            },
            {
              label: 'REJECT',
              onClick: () => {
                const targetStatus = product.subscribers.filter((u) => u.id === cell.original.id)[0]
                  .requestedIsSubscribedStatus;
                const user = {
                  ...cell.original,
                  isSubscribed: !targetStatus,
                  requestedIsSubscribedStatus: targetStatus,
                  subscriptionChangeActioned: true,
                };
                if (product.subscribers.some((u) => u.id === cell.original.id)) {
                  setFieldValue(
                    'subscribers',
                    product.subscribers.map((item) => (item.id === cell.original.id ? user : item)),
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
