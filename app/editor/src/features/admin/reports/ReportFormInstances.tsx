import { useFormikContext } from 'formik';
import React from 'react';
import { useReports } from 'store/hooks/admin';
import { FlexboxTable, IReportInstanceModel, IReportModel } from 'tno-core';

import { instanceColumns } from './constants';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormInstance: React.FC = () => {
  const { values } = useFormikContext<IReportModel>();
  const [, { findInstancesForReportId }] = useReports();

  const [instances, setInstances] = React.useState<IReportInstanceModel[]>([]);

  React.useEffect(() => {
    findInstancesForReportId(values.id).then((instances) => {
      setInstances(instances);
    });
    // Only load the report instances when clicking on the tab.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <>
      <h2>{values.name}</h2>
      <p>
        A report instance is a when a user has published a report, or is actively adding content to
        a report. Each instance provides an historical archive of what content was included in the
        report, and information related to the success or failure of the sent emails.
      </p>
      <FlexboxTable rowId="id" data={instances} columns={instanceColumns()} />
    </>
  );
};
