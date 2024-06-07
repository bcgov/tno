import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  CellEllipsis,
  Checkbox,
  EmailSendToName,
  FormikSelect,
  getEnumStringOptions,
  Grid,
  IUserAVOverviewModel,
  IUserFilter,
  OptionItem,
  SortDirection,
} from 'tno-core';

import { IAVOverviewTemplateForm } from './interfaces';
import { ListFilter } from './ListFilter';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const OverviewSubscribers: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewTemplateForm>();
  const [{ users }, { findUsers }] = useUsers();
  const sendToOptions = getEnumStringOptions(EmailSendToName, { splitOnCapital: false });

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
    <>
      <ListFilter
        onSearch={async (value: string) => {
          await findUsers({ page: 1, quantity: users.quantity, keyword: value });
        }}
      />
      <Grid
        items={users.items.map<IUserAVOverviewModel>((u) => ({
          ...u,
          isSubscribed: values.subscribers.some((s) => s.id === u.id && s.isSubscribed),
          sendTo: values.subscribers.find((s) => s.id === u.id)?.sendTo ?? EmailSendToName.To,
        }))}
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
          { name: 'sendTo', label: 'Send as' },
        ]}
        renderColumns={(row: IUserAVOverviewModel, rowIndex) => [
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
          <FormikSelect
            key=""
            name={`subscribers.${rowIndex}.format`}
            options={sendToOptions}
            value={sendToOptions.find((o) => o.value === row.sendTo) ?? ''}
            onChange={(e) => {
              const option = e as OptionItem;
              if (option) {
                if (values.subscribers.some((u) => u.id === row.id)) {
                  const user = { ...row, sendTo: option.value };
                  setFieldValue(
                    'subscribers',
                    values.subscribers.map((item) => (item.id === row.id ? user : item)),
                  );
                } else {
                  const user = {
                    ...row,
                    isSubscribed: true,
                    sendTo: option.value,
                  };
                  setFieldValue('subscribers', [user, ...values.subscribers]);
                }
              }
            }}
            isClearable={false}
          />,
        ]}
      />
    </>
  );
};
