import { Grid } from 'components/grid';
import { SortDirection } from 'components/grid/SortAction';
import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  CellEllipsis,
  Checkbox,
  FormikSelect,
  getEnumStringOptions,
  IReportModel,
  IUserFilter,
  IUserReportModel,
  OptionItem,
  ReportDistributionFormatName,
} from 'tno-core';

import { ListFilter } from './ListFilter';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormSubscribers: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ users }, { findUsers }] = useUsers();
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);

  const [filter, setFilter] = React.useState<IUserFilter>({ page: 1, quantity: 100, sort: [] });

  const subscribers = users.items.map<IUserReportModel>((u) => {
    const subscriber = values.subscribers.find((s) => s.userId === u.id);
    return {
      ...u,
      userId: u.id,
      reportId: values.id,
      isSubscribed: subscriber?.isSubscribed ?? false,
      format: subscriber?.format ?? ReportDistributionFormatName.LinkOnly,
      version: 0,
    };
  });
  const page = { ...users, items: subscribers };

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
    <>
      <h2>{values.name}</h2>
      <ListFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <Grid
        data={page}
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
          { name: 'format', label: 'Format' },
          { name: 'username', label: 'Username', sortable: true },
          { name: 'lastName', label: 'Last Name', sortable: true },
          { name: 'firstName', label: 'First Name', sortable: true },
          { name: 'email', label: 'Email', sortable: true },
        ]}
        renderRow={(row, rowIndex) => [
          <Checkbox
            key=""
            name={`chk-${row.userId}`}
            checked={values.subscribers.some((u) => u.userId === row.userId && u.isSubscribed)}
            onChange={(e) => {
              const user = { ...row, isSubscribed: e.target.checked };
              if (values.subscribers.some((u) => u.userId === user.userId))
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) => (item.userId === user.userId ? user : item)),
                );
              else setFieldValue('subscribers', [user, ...values.subscribers]);
            }}
          />,
          <FormikSelect
            key=""
            name={`subscribers.${rowIndex}.format`}
            options={formatOptions}
            value={formatOptions.find((o) => o.value === row.format) ?? ''}
            onChange={(e) => {
              const option = e as OptionItem;
              if (option) {
                if (values.subscribers.some((u) => u.userId === row.userId)) {
                  const user = { ...row, format: option.value };
                  setFieldValue(
                    'subscribers',
                    values.subscribers.map((item) => (item.userId === row.userId ? user : item)),
                  );
                } else {
                  const user = {
                    ...row,
                    isSubscribed: true,
                    format: option.value,
                  };
                  setFieldValue('subscribers', [user, ...values.subscribers]);
                }
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
        ]}
      />
    </>
  );
};
