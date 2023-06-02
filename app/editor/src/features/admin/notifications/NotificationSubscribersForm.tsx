import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  FlexboxTable,
  INotificationModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserModel,
} from 'tno-core';

import { subscriberColumns } from './constants';
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

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserModel>) => {
      await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (sort: ITableSort<IUserModel>[], table: ITableInternal<IUserModel>) => {
      const sorts = sort
        .filter((s) => s.isSorted)
        .map((s) => `${s.id}${s.isSortedDesc ? ' desc' : ''}`);
      await findUsers({ page: 1, quantity: users.quantity, sort: sorts });
    },
    [findUsers, users.quantity],
  );

  return (
    <div>
      <NotificationFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <FlexboxTable
        rowId="id"
        columns={subscriberColumns(values, setFieldValue)}
        data={users.items}
        manualPaging
        pageIndex={users.page}
        pageSize={users.quantity}
        pageCount={Math.ceil(users.total / users.quantity)}
        onPageChange={handlePageChange}
        onSortChange={handleSortChange}
        showSort
      />
    </div>
  );
};
