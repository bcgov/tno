import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  FlexboxTable,
  IProductModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserSubscriberModel,
} from 'tno-core';

import { subscriberColumns } from './constants';
import { ProductFilter } from './ProductFilter';
import * as styled from './styled';

export const ProductSubscribersForm = () => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();
  const [{ users }, { findUsers }] = useUsers();

  React.useEffect(() => {
    findUsers({}).catch(() => {
      // Handled already.
    });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserSubscriberModel>) => {
      await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (
      sort: ITableSort<IUserSubscriberModel>[],
      table: ITableInternal<IUserSubscriberModel>,
    ) => {
      const sorts = sort
        .filter((s) => s.isSorted)
        .map((s) => `${s.id}${s.isSortedDesc ? ' desc' : ''}`);
      await findUsers({ page: 1, quantity: users.quantity, sort: sorts });
    },
    [findUsers, users.quantity],
  );

  return (
    <styled.ProductSubscribersForm>
      <ProductFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <FlexboxTable
        rowId="id"
        columns={subscriberColumns(values, setFieldValue)}
        data={users.items as IUserSubscriberModel[]}
        manualPaging
        pageIndex={users.page}
        pageSize={users.quantity}
        pageCount={Math.ceil(users.total / users.quantity)}
        onPageChange={handlePageChange}
        onSortChange={handleSortChange}
        showSort
      />
    </styled.ProductSubscribersForm>
  );
};
