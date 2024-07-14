import React from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';

import { ReportInstanceView } from './ReportInstanceView';

export interface IReportViewProps {
  // Whether to regenerate the report instance.
  regenerate?: boolean;
}

export const ReportView: React.FC<IReportViewProps> = ({ regenerate = false }) => {
  const [, { generateReport }] = useReports();
  const { id } = useParams();

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
          if (report.instances.length) setInstanceId(report.instances[0].id);
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

  return <ReportInstanceView instanceId={instanceId} regenerate={regenerate} />;
};
