import React from 'react';
import { useReports } from 'store/hooks/admin';
import { IReportModel } from 'tno-core';

import { ReportCard } from './ReportCard';
import * as styled from './styled';

// TODO: Display all enabled reports
// Display all schedules to determine when the report should have run, or will run
// Display all report instances to determine if the report has run
// Display who received emails
// Display status of CHES emails

export const ReportDashboard: React.FC = () => {
  const [, { getDashboard }] = useReports();

  const [reports, setReports] = React.useState<IReportModel[]>([]);

  React.useEffect(() => {
    getDashboard().then((dashboard) => {
      setReports(dashboard.reports);
    });
    // Only on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.ReportDashboard>
      <h1>Report Dashboard</h1>
      <div className="header">
        <div>Name</div>
        <div>Owner</div>
        <div>Status</div>
        <div>Last Sent</div>
        <div>Next Run</div>
        <div></div>
      </div>
      <div className="report-cards">
        {reports.map((report) => {
          return <ReportCard key={report.id} report={report} />;
        })}
      </div>
    </styled.ReportDashboard>
  );
};

export default ReportDashboard;
