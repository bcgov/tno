import { useFormikContext } from 'formik';
import React from 'react';
import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from './constants/defaultReportSchedule';
import { ReportSchedule } from './ReportSchedule';
import * as styled from './styled';

export const ReportFormScheduler: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  React.useEffect(() => {
    // Default to two events if they don't already exist.
    if (values.events.length !== 2) {
      setFieldValue('events', [
        defaultReportSchedule('Schedule 1', values),
        defaultReportSchedule('Schedule 2', values),
      ]);
    }
  }, [setFieldValue, values]);

  return (
    <styled.ReportFormScheduler>
      <h2>{values.name}</h2>
      <p>
        Schedule the report to automatically run and send. Configure at what time and on which day
        of the week the report should be run.
      </p>
      <ReportSchedule label="Schedule 1" index={0} />
      <hr />
      <ReportSchedule label="Schedule 2" index={1} />
      <hr />
    </styled.ReportFormScheduler>
  );
};
