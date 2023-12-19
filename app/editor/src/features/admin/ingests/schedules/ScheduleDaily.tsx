import { getIn, useFormikContext } from 'formik';
import React from 'react';
import {
  FieldSize,
  FormikCheckbox,
  FormikHidden,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTextArea,
  FormikTimeInput,
  IIngestModel,
  ScheduleTypeName,
  ScheduleWeekDayName,
  selectWeekDays,
  useNamespace,
} from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';

import { defaultSchedule } from '../constants';
import * as styled from './styled';

interface IScheduleDailyProps {
  index: number;
  scheduleType?: ScheduleTypeName;
}

export const ScheduleDaily: React.FC<IScheduleDailyProps> = ({
  index,
  scheduleType = ScheduleTypeName.Daily,
}) => {
  const { setFieldValue, values } = useFormikContext<IIngestModel>();
  const { field } = useNamespace('schedules', index);
  const delayMS = getIn(values, field('delayMS'), '');

  React.useEffect(() => {
    if (
      values.scheduleType === ScheduleTypeName.Daily &&
      (values.schedules.length === 0 || values.schedules.length > 1)
    )
      setFieldValue(`schedules`, [defaultSchedule()]);
  }, [setFieldValue, values.scheduleType, values.schedules.length]);

  return (
    <styled.ScheduleForm className="schedule">
      <FormikHidden name={field('scheduleType')} value={scheduleType} />
      <Row>
        <Col grow={2}>
          <Row alignItems="center" nowrap>
            <FormikText label="Name" name={field('name')} required />
            <FormikCheckbox label="Enabled" name={field('isEnabled')} />
          </Row>
          <FormikTextArea label="Description" name={field('description')} />
          <Row nowrap className="timing">
            <p>Start the service at</p>
            <FormikTimeInput
              name={field('startAt')}
              value={getIn(values, field('startAt')) ?? ''}
              width="7em"
              onChange={(e: any) => setFieldValue(field('startAt'), e.target.value)}
              placeholder="HH:MM:SS"
              required
            />
            <p>and stop it at</p>
            <FormikTimeInput
              name={field('stopAt')}
              value={getIn(values, field('stopAt')) ?? ''}
              width="7em"
              onChange={(e: any) => setFieldValue(field('stopAt'), e.target.value)}
              placeholder="HH:MM:SS"
              required
            />
            <p>on the following days;</p>
          </Row>
          <Row>
            <FormikText
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
        <Row>
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
