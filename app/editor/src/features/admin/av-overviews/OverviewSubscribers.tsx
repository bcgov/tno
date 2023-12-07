import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  FlexboxTable,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserAVOverviewModel,
} from 'tno-core';

import { subscriberColumns } from './constants';
import { IAVOverviewTemplateForm } from './interfaces';
import { ListFilter } from './ListFilter';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const OverviewSubscribers: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewTemplateForm>();
  const [{ users }, { findUsers }] = useUsers();

  React.useEffect(() => {
    findUsers({ page: 1, quantity: users.quantity }).catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserAVOverviewModel>) => {
      try {
        await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
      } catch {}
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (
      sort: ITableSort<IUserAVOverviewModel>[],
      table: ITableInternal<IUserAVOverviewModel>,
    ) => {
      const sorts = sort
        .filter((s) => s.isSorted)
        .map((s) => `${s.id}${s.isSortedDesc ? ' desc' : ''}`);
      try {
        await findUsers({ page: 1, quantity: users.quantity, sort: sorts });
      } catch {}
    },
    [findUsers, users.quantity],
  );

  return (
    <>
      <ListFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <FlexboxTable
        rowId="id"
        columns={subscriberColumns(values, setFieldValue)}
        data={users.items.map<IUserAVOverviewModel>((u) => ({
          ...u,
          isSubscribed: values.subscribers.some((s) => s.id === u.id && s.isSubscribed),
        }))}
        manualPaging
        pageIndex={users.page - 1}
        pageSize={users.quantity}
        pageCount={Math.ceil(users.total / users.quantity)}
        onPageChange={handlePageChange}
        onSortChange={handleSortChange}
        showSort
      />
    </>
  );
};
