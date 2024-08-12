import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  CellEllipsis,
  Checkbox,
  FormikSelect,
  getEnumStringOptions,
  Grid,
  IProductModel,
  IUserFilter,
  IUserModel,
  OptionItem,
  ProductTypeName,
  ReportDistributionFormatName,
  SortDirection,
} from 'tno-core';

import { ProductFilter } from './ProductFilter';
import * as styled from './styled';

export const ProductSubscribersForm = () => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();
  const [{ users }, { findUsers }] = useUsers();
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);

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

  React.useEffect(() => {
    findUsers({}).catch(() => {
      // Handled already.
    });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.ProductSubscribersForm>
      <ProductFilter
        productId={values.id}
        onSearch={async (value: string, isSubscribedToProductId) => {
          await findUsers({
            page: 1,
            quantity: users.quantity,
            keyword: value,
            isSubscribedToProductId: isSubscribedToProductId,
          });
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
        renderHeader={() => {
          const columns = [
            { name: 'isSubscribed', label: '', size: '30px' },
            { name: 'format', label: 'Format', size: '10%' },
            { name: 'username', label: 'Username', size: '14%', sortable: true },
            { name: 'lastName', label: 'Last Name', size: '17%', sortable: true },
            { name: 'firstName', label: 'First Name', size: '17%', sortable: true },
            { name: 'email', label: 'Email', size: '40%', sortable: true },
          ];
          if (values.productType !== ProductTypeName.Report) columns.splice(1, 1);
          return columns;
        }}
        renderColumns={(row: IUserModel, rowIndex) => {
          const subscriber = values.subscribers.find((u) => u.id === row.id);
          const columns = [
            <Checkbox
              key=""
              name={`chk-${row.id}`}
              checked={subscriber?.isSubscribed ?? false}
              onChange={(e) => {
                const user = { ...row, ...subscriber, isSubscribed: e.currentTarget.checked };
                if (values.subscribers.some((u) => u.id === user.id))
                  setFieldValue(
                    'subscribers',
                    values.subscribers.map((item) => (item.id === user.id ? user : item)),
                  );
                else setFieldValue('subscribers', [user, ...values.subscribers]);
              }}
            />,
            <FormikSelect
              key=""
              name={`subscribers.${rowIndex}.format`}
              options={formatOptions}
              value={formatOptions.find((o) => o.value === subscriber?.format) ?? ''}
              onChange={(e) => {
                const option = e as OptionItem;
                const subscriber = values.subscribers.find((u) => u.id === row.id);
                if (subscriber) {
                  const user = { ...subscriber, isSubscribed: true, format: option.value };
                  setFieldValue(
                    'subscribers',
                    values.subscribers.map((item) => (item.id === row.id ? user : item)),
                  );
                } else {
                  const user = {
                    ...row,
                    isSubscribed: true,
                    format: option.value,
                  };
                  setFieldValue('subscribers', [user, ...values.subscribers]);
                }
              }}
              isClearable={false}
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
          ];
          if (values.productType !== ProductTypeName.Report) columns.splice(1, 1);
          return columns;
        }}
      />
    </styled.ProductSubscribersForm>
  );
};
