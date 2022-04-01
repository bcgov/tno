import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, IScheduleModel, ScheduleType } from 'hooks/api-editor';
import React from 'react';
import { Button, ButtonVariant } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { GridTable } from 'tno-core/dist/components/grid-table';

import { defaultSchedule } from '../constants';
import { ScheduleSingle } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IScheduleProgramProps {
  message?: string;
}

export const ScheduleProgram: React.FC<IScheduleProgramProps> = ({ message }) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules');

  const [index, setIndex] = React.useState<number>();
  const [schedule, setSchedule] = React.useState<IScheduleModel>();

  React.useEffect(() => {
    // Set the default values for the initial schedule.
    // TODO: Rethink this entire schedule UI.  It's confusing to have a default schedule.
    const scheduleType = getIn(values, field('scheduleType', 0));
    const startAt = getIn(values, field('startAt', 0));
    const stopAt = getIn(values, field('stopAt', 0));

    if (scheduleType !== ScheduleType.Managed) {
      setFieldValue(field('scheduleType', 0), ScheduleType.Managed);
    }
    if (startAt === undefined) {
      setFieldValue(field('startAt', 0), '08:00:00');
    }
    if (stopAt === undefined) {
      setFieldValue(field('stopAt', 0), '18:00:00');
    }
  }, [field, setFieldValue, values]);

  const handleAdd = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue(`schedules`, [...values.schedules, defaultSchedule]);
    setIndex(values.schedules.length);
  };

  const handleDone = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setIndex(undefined);
  };

  const handleRemove = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue(
      `schedules`,
      values.schedules.filter((s, i) => i !== index),
    );
    setIndex(undefined);
  };

  const handleCancel = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue(`schedules.${index}`, schedule);
    setIndex(undefined);
  };

  return (
    <styled.Schedule className="schedule" alignItems="center">
      <Col>
        {message && <p>{message}</p>}
        <GridTable
          columns={columns}
          data={values.schedules}
          onRowClick={(row) => {
            setIndex(index !== row.index ? row.index : undefined);
            setSchedule(values.schedules[row.index]);
          }}
          paging={{ showPaging: false }}
        ></GridTable>
        <Col alignItems="flex-end">
          {index !== undefined && <ScheduleSingle index={index} />}
          <Row>
            {index === undefined && <Button onClick={handleAdd}>Add</Button>}
            {index !== undefined && <Button onClick={handleDone}>Done</Button>}
            {index !== undefined && (
              <Button variant={ButtonVariant.danger} onClick={handleRemove}>
                Remove
              </Button>
            )}
            {index !== undefined && (
              <Button variant={ButtonVariant.secondary} onClick={handleCancel}>
                Cancel
              </Button>
            )}
          </Row>
        </Col>
      </Col>
    </styled.Schedule>
  );
};
