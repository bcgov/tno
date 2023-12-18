import { getIn, useFormikContext } from 'formik';
import React from 'react';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikHidden,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTextArea,
  IIngestModel,
  Row,
  ScheduleTypeName,
  ScheduleWeekDayName,
  selectWeekDays,
  Text,
  useNamespace,
} from 'tno-core';

import { defaultSchedule } from '../constants';
import * as styled from './styled';

interface IScheduleContinuousProps {}

export const ScheduleContinuous: React.FC<IScheduleContinuousProps> = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const { field } = useNamespace('schedules', 0);
  const delayMS = getIn(values, field('delayMS'), '');

  React.useEffect(() => {
    if (values.schedules.length === 0 || values.schedules.length > 1)
      setFieldValue(`schedules`, [
        {
          ...defaultSchedule(),
          startAt: '00:00:00',
          stopAt: '23:59:59',
          delayMS: '',
        },
      ]);
  }, [setFieldValue, values.schedules.length]);

  return (
    <styled.ScheduleForm className="schedule">
      <FormikHidden name={field('scheduleType')} value={ScheduleTypeName.Continuous} />
      <Row>
        <Col grow={2}>
          <Row alignItems="center" nowrap>
            <FormikText label="Name" name={field('name')} required />
            <FormikCheckbox label="Enabled" name={field('isEnabled')} />
          </Row>
          <FormikTextArea label="Description" name={field('description')} />
          <Row nowrap className="timing">
            <Text
              name={field('delayMS')}
              label="Delay (seconds)"
              type="number"
              required
              width={FieldSize.Tiny}
              value={!!delayMS ? +delayMS / 1000 : ''}
              min={1}
              onChange={(e) => {
                const value = Number(e.target.value) * 1000;
                setFieldValue(field('delayMS'), value);
              }}
            />
            <p>Control how often the service will perform an action.</p>
          </Row>
        </Col>
        <Row grow={1}>
          <label>Run On</label>
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Monday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Monday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Tuesday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Tuesday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Wednesday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Wednesday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Thursday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Thursday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Friday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Friday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Saturday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Saturday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Sunday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Sunday}
            onBeforeChange={(value) => selectWeekDays(value)}
          />
        </Row>
      </Row>
    </styled.ScheduleForm>
  );
};
