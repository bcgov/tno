import { setIn, setNestedObjectValues, useFormikContext, type FormikTouched } from 'formik';
import React from 'react';
import { ValidationError } from 'yup';

import {
  Button,
  ButtonVariant,
  ScheduleTypeName,
  useNamespace,
  type IIngestModel,
  type IScheduleModel,
} from '@/shared';
import { Col, Row } from '@/shared/components/flex';
import { FlexboxTable } from '@/shared/components/table';

import { ScheduleDaily } from '.';
import { defaultSchedule } from '../constants';
import { columns } from './constants';
import * as styled from './styled';

interface IScheduleAdvancedProps {}

export const ScheduleAdvanced: React.FC<IScheduleAdvancedProps> = () => {
  const { values, setFieldValue, validateForm, setTouched } = useFormikContext<IIngestModel>();
  const { field } = useNamespace('schedules');

  const [index, setIndex] = React.useState<number>();
  const [schedule, setSchedule] = React.useState<IScheduleModel>();

  const handleAdd = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue('schedules', [...values.schedules, defaultSchedule()]);
    setIndex(values.schedules.length);
  };

  const handleDone = async (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    if (index !== undefined) {
      try {
        // Mock what formik does on submit, values must be "touched" for validation to fire.
        const errors = await validateForm();
        if (Object.keys(errors).length === 0) {
          setIndex(undefined);
        } else {
          setTouched(setNestedObjectValues<FormikTouched<IIngestModel>>(errors, true));
        }
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
      'schedules',
      values.schedules.filter((s, i) => i !== index),
    );
    setIndex(undefined);
  };

  const handleCancel = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    setFieldValue(`schedules.${index}`, schedule);
    setIndex(undefined);
  };

  return (
    <styled.ScheduleForm className="schedule">
      <FlexboxTable
        className="schedules"
        rowId={(schedule) => `${schedule ? values.schedules.indexOf(schedule) : ''}`}
        columns={columns}
        data={values.schedules}
        onRowClick={(row) => {
          setIndex(index !== row.index ? row.index : undefined);
          setSchedule(values.schedules[row.index]);
        }}
        pagingEnabled={false}
      />
      <Col className="actions">
        {index !== undefined && (
          <ScheduleDaily index={index} scheduleType={ScheduleTypeName.Advanced} />
        )}
        <Row alignItems="flex-end" style={{ marginLeft: 'auto' }}>
          {index === undefined && (
            <Button onClick={handleAdd} variant={ButtonVariant.secondary}>
              Add
            </Button>
          )}
          {index !== undefined && (
            <Button onClick={handleDone} variant={ButtonVariant.secondary}>
              Done
            </Button>
          )}
          {index !== undefined && (
            <Button variant={ButtonVariant.danger} onClick={handleRemove}>
              Remove
            </Button>
          )}
          {index !== undefined && values.schedules[index].id !== 0 && (
            <Button variant={ButtonVariant.secondary} onClick={handleCancel}>
              Cancel
            </Button>
          )}
        </Row>
      </Col>
    </styled.ScheduleForm>
  );
};
