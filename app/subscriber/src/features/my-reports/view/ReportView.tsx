import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';

import { ReportInstanceView } from './components';
import * as styled from './styled';

/**
 * Provides a way to view the generate report instance.
 * If a report is requested it will generate a report instance before viewing.
 * @returns Component
 */
export const ReportView: React.FC = () => {
  const [{ generateReport }] = useReports();
  const { id } = useParams();
  const navigate = useNavigate();

  const reportId = parseInt(id ?? '');
  const isInstance = () => {
    return window.location.pathname.includes('/report/instances/');
  };
  const [instanceId, setInstanceId] = React.useState(isInstance() ? reportId : 0);
  const [generate, setGenerate] = React.useState(true);

  React.useEffect(() => {
    setInstanceId(isInstance() ? parseInt(id ?? '') : 0);
  }, [id]);

  React.useEffect(() => {
    if (reportId && generate && !isInstance()) {
      generateReport(reportId)
        .then((report) => {
          if (report.instances.length) navigate(`/report/instances/${report.instances[0].id}/view`);
          else toast.error('An unexpected error occurred while generating the report');
        })
        .catch(() => {})
        .finally(() => {
          setGenerate(false);
        });
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [reportId]);

  return (
    <styled.ReportView>
      <Header />
      <PageSection>
        <ReportInstanceView instanceId={instanceId} />
      </PageSection>
    </styled.ReportView>
  );
};
