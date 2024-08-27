import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { formatDate, Grid, IUserModel, IUserProductModel, Link } from 'tno-core';

export const UserProductSubscriptions: React.FC = () => {
  const { values } = useFormikContext<IUserModel>();
  const [, { getUserProductSubscriptions }] = useUsers();

  const [subscriptions, setSubscriptions] = React.useState<IUserProductModel[]>([]);

  React.useEffect(() => {
    if (!subscriptions.length && values.id) {
      getUserProductSubscriptions(values.id)
        .then((data) => setSubscriptions(data))
        .catch(() => {});
    }
  }, [getUserProductSubscriptions, subscriptions.length, values.id]);

  return (
    <div className="subscriber-list">
      <Grid
        items={subscriptions}
        showPaging={false}
        onSortChange={async (column, direction) => {}}
        renderHeader={() => [
          { name: 'product.name', label: 'Name' },
          { name: 'createdOn', label: 'Created', size: '120px' },
          { name: 'updatedOn', label: 'Updated', size: '120px' },
        ]}
        renderColumns={(row: IUserProductModel, rowIndex) => [
          <div key="1">
            <Link to={`/admin/products/${row.productId}`}>{row.product?.name}</Link>
          </div>,
          <div key="2">{formatDate(row.createdOn, 'YYYY-MM-DD')}</div>,
          <div key="3">{formatDate(row.updatedOn, 'YYYY-MM-DD')}</div>,
        ]}
      />
    </div>
  );
};
