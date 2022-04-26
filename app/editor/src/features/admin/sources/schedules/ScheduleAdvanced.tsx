import { FormikHidden } from 'components/formik';
import { setIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, IScheduleModel, ScheduleTypeName } from 'hooks/api-editor';
import React from 'react';
import { Button, ButtonVariant } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { GridTable } from 'tno-core/dist/components/grid-table';
import { ValidationError } from 'yup';

import { defaultSchedule } from '../constants';
import { ScheduleDaily } from '.';
import { columns } from './constants';
import * as styled from './styled';
import { ScheduleSchema } from './validation';

interface IScheduleAdvancedProps {}

export const ScheduleAdvanced: React.FC<IScheduleAdvancedProps> = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules');

  const [index, setIndex] = React.useState<number>();
  const [schedule, setSchedule] = React.useState<IScheduleModel>();

  const handleAdd = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue(`schedules`, [...values.schedules, defaultSchedule]);
    setIndex(values.schedules.length);
  };

  const handleDone = async (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    if (!!index) {
      try {
        // TODO: Validation doesn't work.
        await ScheduleSchema.validate(values.schedules[index], { abortEarly: false });
        setIndex(undefined);
      } catch (error) {
        if (error instanceof ValidationError) {
          error.inner.reduce((formError, innerError) => {
            return setIn(formError, field(innerError.path ?? '', index), innerError.message);
          }, {});
        }
      }
    }
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
      <GridTable
        className="schedules"
        columns={columns}
        data={values.schedules}
        onRowClick={(row) => {
          setIndex(index !== row.index ? row.index : undefined);
          setSchedule(values.schedules[row.index]);
        }}
        paging={{ showPaging: false }}
      ></GridTable>
      <Col className="actions">
        {index !== undefined && <ScheduleDaily index={index} />}
        <Row alignItems="flex-end" style={{ marginLeft: 'auto' }}>
          {index === undefined && <Button onClick={handleAdd}>Add</Button>}
          {index !== undefined && (
            <Button onClick={handleDone}>
              {values.schedules[index].id === 0 ? 'Add' : 'Update'}
            </Button>
          )}
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
    </styled.Schedule>
  );
};
