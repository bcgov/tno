import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { formatDate, Grid, type IUserModel, type IUserNotificationModel, Link } from 'tno-core';

/** A notification subscription, which belongs to a distribution list when 'userId' is not the user's. */
type IUserNotificationSubscription = IUserNotificationModel & { userId: number };

export const UserNotificationSubscriptions: React.FC = () => {
  const { values } = useFormikContext<IUserModel>();
  const [, { getUserNotificationSubscriptions }] = useUsers();

  const [subscriptions, setSubscriptions] = React.useState<IUserNotificationSubscription[]>([]);

  React.useEffect(() => {
    if (subscriptions.length === 0 && values.id) {
      getUserNotificationSubscriptions(values.id)
        .then((data) => {
          setSubscriptions(data as IUserNotificationSubscription[]);
        })
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
          { name: 'userId', label: 'Received through' },
          { name: 'createdOn', label: 'Created', size: '120px' },
          { name: 'updatedOn', label: 'Updated', size: '120px' },
        ]}
        renderColumns={(row: IUserNotificationSubscription, rowIndex) => [
          <div key="1">
            <Link to={`/admin/notifications/${row.notificationId}`}>{row.notification?.name}</Link>
          </div>,
          <div key="2">
            {row.userId === values.id ? (
              'Direct subscription'
            ) : (
              <Link to={`/admin/users/${row.userId}`}>
                Distribution list: {row.displayName || row.username}
              </Link>
            )}
          </div>,
          <div key="3">{formatDate(row.createdOn, 'YYYY-MM-DD')}</div>,
          <div key="4">{formatDate(row.updatedOn, 'YYYY-MM-DD')}</div>,
        ]}
      />
    </div>
  );
};
