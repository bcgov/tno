import { FieldSize, Text } from 'components/form';
import {
  FormikCheckbox,
  FormikHidden,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, ScheduleType, ScheduleWeekDayName } from 'hooks/api-editor';
import React from 'react';
import { Col, Row } from 'tno-core/dist/components/flex';

import { defaultSchedule } from '../constants';
import * as styled from './styled';

interface IScheduleContinuousProps {
  index: number;
  message?: string;
}

export const ScheduleContinuous: React.FC<IScheduleContinuousProps> = ({ index, message }) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules', index);

  const schedule = values.schedules.length > index ? values.schedules[index] : defaultSchedule;
  const runOnWeekDays = getIn(values, field('runOnWeekDays'), ScheduleWeekDayName.NA);

  return (
    <styled.Schedule className="schedule">
      <Col>
        {message && <p>{message}</p>}
        <FormikHidden name={field('scheduleType')} value={ScheduleType.Repeating} />
        <Row alignItems="center" nowrap>
          <FormikText label="Name" name={field('name')} required />
          <FormikCheckbox label="Enabled" name={field('isEnabled')} />
        </Row>
        <FormikTextArea label="Description" name={field('description')} />
        <Row nowrap>
          <p>Run service every</p>
          <Text
            name={field('delayMS')}
            type="number"
            required
            width={FieldSize.Tiny}
            value={schedule.delayMS / 1000}
            min={1}
            onChange={(e) => {
              const value = Number(e.target.value) * 1000;
              setFieldValue(field('delayMS'), value);
            }}
          />
          <p>seconds, on the following days;</p>
        </Row>
        <Col>
          <FormikStringEnumCheckbox<ScheduleWeekDayName>
            label="Monday"
            name={field('runOnWeekDays')}
            value={ScheduleWeekDayName.Monday}
            checked={runOnWeekDays.includes(ScheduleWeekDayName.Monday)}
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
        </Col>
      </Col>
    </styled.Schedule>
  );
};
