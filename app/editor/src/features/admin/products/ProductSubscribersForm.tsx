import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  FlexboxTable,
  IProductModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserProductModel,
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
    async (page: ITablePage, table: ITableInternal<IUserProductModel>) => {
      await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (sort: ITableSort<IUserProductModel>[], table: ITableInternal<IUserProductModel>) => {
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
        data={users.items as IUserProductModel[]}
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
