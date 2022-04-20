import { FieldSize, TimeInput } from 'components/form';
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

import * as styled from './styled';

interface IScheduleSingleProps {
  index: number;
  message?: string;
}

export const ScheduleSingle: React.FC<IScheduleSingleProps> = ({ index, message }) => {
  const { setFieldValue, values } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules', index);

  React.useEffect(() => {
    setFieldValue(field('scheduleType'), ScheduleType.Managed);
  }, [field, index, setFieldValue]);

  return (
    <styled.Schedule className="schedule" alignItems="center">
      <Col>
        {message && <p>{message}</p>}
        <FormikHidden name={field('scheduleType')} value={ScheduleType.Managed} />
        <Row alignItems="center" nowrap>
          <FormikText label="Name" name={field('name')} required />
          <FormikCheckbox label="Enabled" name={field('isEnabled')} />
        </Row>
        <FormikTextArea label="Description" name={field('description')} />
        <Row nowrap>
          <p>Start the service at</p>
          <TimeInput
            name={field('startAt')}
            value={getIn(values, field('startAt'))}
            width={FieldSize.Small}
            onChange={(e: any) => setFieldValue(field('startAt'), e.target.value)}
            placeholder="HH:MM:SS"
            required
          />
          <p>and stop it at</p>
          <TimeInput
            name={field('stopAt')}
            value={getIn(values, field('stopAt'))}
            width={FieldSize.Small}
            onChange={(e: any) => setFieldValue(field('stopAt'), e.target.value)}
            placeholder="HH:MM:SS"
            required
          />
          <p>on the following days;</p>
        </Row>
        <Col>
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
        </Col>
      </Col>
    </styled.Schedule>
  );
};
