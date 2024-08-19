import { useFormikContext } from 'formik';
import {
  CellEllipsis,
  Checkbox,
  Grid,
  IProductModel,
  IUserFilter,
  IUserProductModel,
  ProductRequestStatusName,
  SortDirection,
} from 'tno-core';

export interface IProductNotificationSubscribersFormProps {
  users: IUserProductModel[];
  page: number;
  quantity: number;
  total: number;
  setFilter: React.Dispatch<React.SetStateAction<IUserFilter>>;
}

export const ProductNotificationSubscribersForm: React.FC<
  IProductNotificationSubscribersFormProps
> = ({ users, page, quantity, total, setFilter }) => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();

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
        { name: 'username', label: 'Username', size: '14%', sortable: true },
        { name: 'lastName', label: 'Last Name', size: '17%', sortable: true },
        { name: 'firstName', label: 'First Name', size: '17%', sortable: true },
        { name: 'email', label: 'Email', size: '40%', sortable: true },
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
          <CellEllipsis key="2">{row.username}</CellEllipsis>,
          <CellEllipsis key="3">{row.lastName}</CellEllipsis>,
          <CellEllipsis key="4">{row.firstName}</CellEllipsis>,
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
