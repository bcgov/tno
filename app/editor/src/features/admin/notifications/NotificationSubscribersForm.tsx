import { Grid } from 'components/grid';
import { SortDirection } from 'components/grid/SortAction';
import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { CellEllipsis, Checkbox, INotificationModel } from 'tno-core';

import { NotificationFilter } from './NotificationFilter';

export const NotificationSubscribersForm = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();
  const [{ users }, { findUsers }] = useUsers();

  React.useEffect(() => {
    findUsers({}).catch(() => {
      // Handled already.
    });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div>
      <NotificationFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <Grid
        data={users}
        showPaging
        onNavigatePage={async (page) => {
          await findUsers({ page, quantity: users.quantity });
        }}
        onQuantityChange={async (quantity) => {
          await findUsers({ page: 1, quantity: quantity });
        }}
        onSortChange={async (column, direction) => {
          await findUsers({
            page: 1,
            quantity: users.quantity,
            sort: direction === SortDirection.None ? [] : [`${column.name} ${direction}`],
          });
        }}
        renderHeader={() => [
          { name: 'isSubscribed', label: '' },
          { name: 'username', label: 'Username', sortable: true },
          { name: 'lastName', label: 'Last Name', sortable: true },
          { name: 'firstName', label: 'First Name', sortable: true },
          { name: 'email', label: 'Email', sortable: true },
        ]}
        renderRow={(row) => [
          <Checkbox
            key=""
            name={`chk-${row.id}`}
            checked={values.subscribers.some((u) => u.id === row.id && u.isSubscribed)}
            onChange={(e) => {
              const user = { ...row, isSubscribed: e.target.checked };
              if (values.subscribers.some((u) => u.id === user.id))
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) => (item.id === user.id ? user : item)),
                );
              else setFieldValue('subscribers', [user, ...values.subscribers]);
            }}
          />,
          <CellEllipsis key="">{row.username}</CellEllipsis>,
          <CellEllipsis key="">{row.lastName}</CellEllipsis>,
          <CellEllipsis key="">{row.firstName}</CellEllipsis>,
          <>
            <CellEllipsis>{row.email}</CellEllipsis>
            {row.preferredEmail ? (
              <CellEllipsis className="preferred">{row.preferredEmail}</CellEllipsis>
            ) : (
              ''
            )}
          </>,
        ]}
      />
    </div>
  );
};
