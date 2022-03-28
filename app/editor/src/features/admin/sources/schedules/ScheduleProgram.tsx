import { GridTable } from 'components/grid-table';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, ScheduleType } from 'hooks/api-editor';
import React from 'react';

import { columns } from './constants';
import * as styled from './styled';

interface IScheduleProgramProps {}

export const ScheduleProgram: React.FC<IScheduleProgramProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules');

  React.useEffect(() => {
    const scheduleType = getIn(values, field('scheduleType', 0));
    const startAt = getIn(values, field('startAt', 0));
    const stopAt = getIn(values, field('stopAt', 0));

    if (scheduleType !== ScheduleType.Managed) {
      setFieldValue(field('scheduleType', 0), ScheduleType.Managed);
    }
    if (startAt === undefined) {
      setFieldValue(field('startAt', 0), '8:00:00');
    }
    if (stopAt === undefined) {
      setFieldValue(field('stopAt', 0), '18:00:00');
    }
  }, [field, setFieldValue, values]);

  return (
    <styled.Schedule className="schedule">
      <GridTable columns={columns} data={values.schedules} onRowClick={(row) => {}}></GridTable>
    </styled.Schedule>
  );
};
