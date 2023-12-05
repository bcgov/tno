import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  FlexboxTable,
  IReportModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserReportModel,
  ReportDistributionFormatName,
} from 'tno-core';

import { useSubscriberColumns } from './hooks';
import { ListFilter } from './ListFilter';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormSubscribers: React.FC = () => {
  const { values } = useFormikContext<IReportModel>();
  const [{ users }, { findUsers }] = useUsers();
  const subscriberColumns = useSubscriberColumns();

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserReportModel>) => {
      await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (sort: ITableSort<IUserReportModel>[], table: ITableInternal<IUserReportModel>) => {
      const sorts = sort
        .filter((s) => s.isSorted)
        .map((s) => `${s.id}${s.isSortedDesc ? ' desc' : ''}`);
      await findUsers({ page: 1, quantity: users.quantity, sort: sorts });
    },
    [findUsers, users.quantity],
  );

  return (
    <>
      <h2>{values.name}</h2>
      <ListFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <FlexboxTable
        rowId="id"
        columns={subscriberColumns}
        data={users.items.map<IUserReportModel>((u) => {
          const subscriber = values.subscribers.find((s) => s.id === u.id);
          return {
            ...u,
            isSubscribed: subscriber?.isSubscribed ?? false,
            format: subscriber?.format ?? ReportDistributionFormatName.LinkOnly,
          };
        })}
        manualPaging
        pageIndex={users.page}
        pageSize={users.quantity}
        pageCount={Math.ceil(users.total / users.quantity)}
        onPageChange={handlePageChange}
        onSortChange={handleSortChange}
        showSort
        showActive={false}
      />
    </>
  );
};
