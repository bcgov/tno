import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { Grid, IUserModel, SortDirection } from 'tno-core';
import { CellEllipsis, Checkbox, INotificationModel, IUserFilter } from 'tno-core';

import { NotificationFilter } from './NotificationFilter';

export const NotificationSubscribersForm = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();
  const [{ users }, { findUsers }] = useUsers();

  const [filter, setFilter] = React.useState<IUserFilter>({ page: 1, quantity: 100, sort: [] });

  const fetch = React.useCallback(
    async (filter: IUserFilter) => {
      try {
        await findUsers(filter);
      } catch {}
    },
    [findUsers],
  );

  React.useEffect(() => {
    fetch(filter).catch();
    // The fetch method will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  return (
    <div>
      <NotificationFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <Grid
        items={users.items}
        pageIndex={users.page - 1}
        itemsPerPage={users.quantity}
        totalItems={users.total}
        showPaging
        onNavigatePage={async (page) => {
          setFilter((filter) => ({ ...filter, page }));
        }}
        onQuantityChange={async (quantity) => {
          setFilter((filter) => ({ ...filter, page: 1, quantity }));
        }}
        onSortChange={async (column, direction) => {
          setFilter((filter) => ({
            ...filter,
            page: 1,
            sort: direction === SortDirection.None ? [] : [`${column.name} ${direction}`],
          }));
        }}
        renderHeader={() => [
          { name: 'isSubscribed', label: '', size: '30px' },
          { name: 'username', label: 'Username', sortable: true },
          { name: 'lastName', label: 'Last Name', sortable: true },
          { name: 'firstName', label: 'First Name', sortable: true },
          { name: 'email', label: 'Email', sortable: true },
        ]}
        renderColumns={(row: IUserModel) => [
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
