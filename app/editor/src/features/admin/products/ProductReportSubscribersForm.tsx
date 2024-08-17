import { useFormikContext } from 'formik';
import {
  CellEllipsis,
  Checkbox,
  EmailSendToName,
  FormikSelect,
  getEnumStringOptions,
  Grid,
  IProductModel,
  IUserFilter,
  IUserProductModel,
  OptionItem,
  ProductRequestStatusName,
  ReportDistributionFormatName,
  SortDirection,
} from 'tno-core';

export interface IProductReportSubscribersFormProps {
  users: IUserProductModel[];
  page: number;
  quantity: number;
  total: number;
  setFilter: React.Dispatch<React.SetStateAction<IUserFilter>>;
}

export const ProductReportSubscribersForm: React.FC<IProductReportSubscribersFormProps> = ({
  users,
  page,
  quantity,
  total,
  setFilter,
}) => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);
  const sendToOptions = getEnumStringOptions(EmailSendToName);

  return (
    <Grid
      items={users}
      pageIndex={page - 1}
      itemsPerPage={quantity}
      totalItems={total}
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
        { name: 'format', label: 'Format', size: '15%' },
        { name: 'sendTo', label: 'Email', size: '10%' },
        { name: 'username', label: 'Username', size: '15%', sortable: true },
        { name: 'lastName', label: 'Last Name', size: 'calc(15% - 15px)', sortable: true },
        { name: 'firstName', label: 'First Name', size: 'calc(15% - 15px)', sortable: true },
        { name: 'email', label: 'Email', size: '30%', sortable: true },
      ]}
      renderColumns={(row: IUserProductModel, rowIndex) => {
        return [
          <Checkbox
            key="1"
            name={`chk-${row.userId}`}
            checked={row.isSubscribed}
            onChange={(e) => {
              const subscription = {
                ...row,
                isSubscribed: e.currentTarget.checked,
                status: ProductRequestStatusName.NA,
              };
              if (values.subscribers.some((u) => u.userId === row.userId))
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) =>
                    item.userId === row.userId ? subscription : item,
                  ),
                );
              else setFieldValue('subscribers', [subscription, ...values.subscribers]);
            }}
          />,
          <FormikSelect
            key="2"
            name={`subscribers.${rowIndex}.format`}
            options={formatOptions}
            value={formatOptions.find((o) => o.value === row.format) ?? ''}
            onChange={(e) => {
              const option = e as OptionItem;
              const subscription = { ...row, isSubscribed: true, format: option.value };
              if (values.subscribers.some((u) => u.userId === row.userId)) {
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) =>
                    item.userId === row.userId ? subscription : item,
                  ),
                );
              } else {
                setFieldValue('subscribers', [subscription, ...values.subscribers]);
              }
            }}
            isClearable={false}
          />,
          <FormikSelect
            key="3"
            name={`subscribers.${rowIndex}.sendTo`}
            options={sendToOptions}
            value={sendToOptions.find((o) => o.value === row.sendTo) ?? ''}
            onChange={(e) => {
              const option = e as OptionItem;
              const subscription = { ...row, isSubscribed: true, sendTo: option.value };
              if (values.subscribers.some((u) => u.userId === row.userId)) {
                setFieldValue(
                  'subscribers',
                  values.subscribers.map((item) =>
                    item.userId === row.userId ? subscription : item,
                  ),
                );
              } else {
                setFieldValue('subscribers', [subscription, ...values.subscribers]);
              }
            }}
            isClearable={false}
          />,
          <CellEllipsis key="4">{row.username}</CellEllipsis>,
          <CellEllipsis key="5">{row.lastName}</CellEllipsis>,
          <CellEllipsis key="6">{row.firstName}</CellEllipsis>,
          <>
            <CellEllipsis>{row.email}</CellEllipsis>
            {row.preferredEmail ? (
              <CellEllipsis className="preferred">{row.preferredEmail}</CellEllipsis>
            ) : (
              ''
            )}
          </>,
        ];
      }}
    />
  );
};
