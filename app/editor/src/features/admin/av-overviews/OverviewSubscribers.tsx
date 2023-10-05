import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { FlexboxTable, ITableInternal, ITablePage, ITableSort, IUserModel } from 'tno-core';

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
    async function getUsers() {
      try {
        await findUsers({ page: 1, quantity: users.quantity });
      } catch {}
    }
    getUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserModel>) => {
      try {
        await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
      } catch {}
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (sort: ITableSort<IUserModel>[], table: ITableInternal<IUserModel>) => {
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
        data={users.items}
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
