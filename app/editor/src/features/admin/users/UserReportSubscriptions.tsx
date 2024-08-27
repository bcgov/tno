import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { formatDate, Grid, IUserModel, IUserReportModel, Link } from 'tno-core';

export const UserReportSubscriptions: React.FC = () => {
  const { values } = useFormikContext<IUserModel>();
  const [, { getUserReportSubscriptions }] = useUsers();

  const [subscriptions, setSubscriptions] = React.useState<IUserReportModel[]>([]);

  React.useEffect(() => {
    if (!subscriptions.length && values.id) {
      getUserReportSubscriptions(values.id)
        .then((data) => setSubscriptions(data))
        .catch(() => {});
    }
  }, [getUserReportSubscriptions, subscriptions.length, values.id]);

  return (
    <div className="subscriber-list">
      <Grid
        items={subscriptions}
        showPaging={false}
        onSortChange={async (column, direction) => {}}
        renderHeader={() => [
          { name: 'report.name', label: 'Name' },
          { name: 'createdOn', label: 'Created', size: '120px' },
          { name: 'updatedOn', label: 'Updated', size: '120px' },
        ]}
        renderColumns={(row: IUserReportModel, rowIndex) => [
          <div key="1">
            <Link to={`/admin/reports/${row.reportId}`}>{row.report?.name}</Link>
          </div>,
          <div key="2">{formatDate(row.createdOn, 'YYYY-MM-DD')}</div>,
          <div key="3">{formatDate(row.updatedOn, 'YYYY-MM-DD')}</div>,
        ]}
      />
    </div>
  );
};
