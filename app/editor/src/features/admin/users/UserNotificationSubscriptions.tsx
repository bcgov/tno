import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { formatDate, Grid, IUserModel, IUserNotificationModel, Link } from 'tno-core';

export const UserNotificationSubscriptions: React.FC = () => {
  const { values } = useFormikContext<IUserModel>();
  const [, { getUserNotificationSubscriptions }] = useUsers();

  const [subscriptions, setSubscriptions] = React.useState<IUserNotificationModel[]>([]);

  React.useEffect(() => {
    if (!subscriptions.length && values.id) {
      getUserNotificationSubscriptions(values.id)
        .then((data) => setSubscriptions(data))
        .catch(() => {});
    }
  }, [getUserNotificationSubscriptions, subscriptions.length, values.id]);

  return (
    <div className="subscriber-list">
      <Grid
        items={subscriptions}
        showPaging={false}
        onSortChange={async (column, direction) => {}}
        renderHeader={() => [
          { name: 'notification.name', label: 'Name' },
          { name: 'createdOn', label: 'Created', size: '120px' },
          { name: 'updatedOn', label: 'Updated', size: '120px' },
        ]}
        renderColumns={(row: IUserNotificationModel, rowIndex) => [
          <div key="1">
            <Link to={`/admin/notifications/${row.notificationId}`}>{row.notification?.name}</Link>
          </div>,
          <div key="2">{formatDate(row.createdOn, 'YYYY-MM-DD')}</div>,
          <div key="3">{formatDate(row.updatedOn, 'YYYY-MM-DD')}</div>,
        ]}
      />
    </div>
  );
};
