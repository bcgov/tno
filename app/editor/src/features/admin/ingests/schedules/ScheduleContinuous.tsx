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
  Text,
  useNamespace,
} from 'tno-core';

import { defaultSchedule } from '../constants';
import * as styled from './styled';

interface IScheduleContinuousProps {
  index: number;
}

export const ScheduleContinuous: React.FC<IScheduleContinuousProps> = ({ index }) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const { field } = useNamespace('schedules', index);
  const delayMS = getIn(values, field('delayMS'), '');

  React.useEffect(() => {
    if (index >= values.schedules.length)
      setFieldValue(`schedules[${index}]`, {
        ...defaultSchedule(ScheduleTypeName.Continuous),
        startAt: '00:00:00',
        stopAt: '23:59:59',
      });
    else if (values.schedules[index].scheduleType !== ScheduleTypeName.Continuous) {
      setFieldValue(`schedules[${index}]`, {
        ...values.schedules[index],
        scheduleType: ScheduleTypeName.Continuous,
        startAt: '00:00:00',
        stopAt: '23:59:59',
        delayMS: '',
      });
    }
  }, [field, index, setFieldValue, values]);

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
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Tuesday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Tuesday}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Wednesday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Wednesday}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Thursday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Thursday}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Friday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Friday}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Saturday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Saturday}
          />
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Sunday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Sunday}
          />
        </Row>
      </Row>
    </styled.ScheduleForm>
  );
};
