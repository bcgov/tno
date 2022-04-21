import { FormikHidden } from 'components/formik';
import { useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, IScheduleModel, ScheduleTypeName } from 'hooks/api-editor';
import React from 'react';
import { Button, ButtonVariant } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { GridTable } from 'tno-core/dist/components/grid-table';

import { defaultSchedule } from '../constants';
import { ScheduleDaily } from '.';
import { columns } from './constants';
import * as styled from './styled';

interface IScheduleAdvancedProps {
  message?: string;
}

export const ScheduleAdvanced: React.FC<IScheduleAdvancedProps> = ({ message }) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules');

  const [index, setIndex] = React.useState<number>();
  const [schedule, setSchedule] = React.useState<IScheduleModel>();

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
      <FormikHidden name={field('scheduleType')} value={ScheduleTypeName.Advanced} />
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
          {index !== undefined && <ScheduleDaily index={index} />}
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
