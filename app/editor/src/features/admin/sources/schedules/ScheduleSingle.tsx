import { Col, Row } from 'components/flex';
import { FieldSize } from 'components/form';
import {
  FormikBitwiseCheckbox,
  FormikCheckbox,
  FormikHidden,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, ScheduleType, WeekDay } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

interface IScheduleSingleProps {
  index: number;
}

export const ScheduleSingle: React.FC<IScheduleSingleProps> = ({ index }) => {
  const { setFieldValue } = useFormikContext<IDataSourceModel>();
  const { field } = useNamespace('schedules', index);

  React.useEffect(() => {
    setFieldValue(field('scheduleType'), ScheduleType.Managed);
  }, [field, index, setFieldValue]);

  return (
    <styled.Schedule className="schedule">
      <p>
        A start & stop service starts at the schedule time and continues to run until the stop time.
      </p>
      <FormikHidden name={field('scheduleType')} value={ScheduleType.Managed} />
      <Row alignItems="center" nowrap>
        <FormikText label="Name" name={field('name')} required />
        <FormikCheckbox label="Enabled" name={field('enabled')} />
      </Row>
      <FormikTextArea label="Description" name={field('description')} />
      <Row nowrap>
        <p>Start the service at</p>
        <FormikText
          name={field('startAt')}
          width={FieldSize.Small}
          placeholder="HH:MM:SS"
          required
        />
        <p>and stop it at</p>
        <FormikText
          name={field('stopAt')}
          width={FieldSize.Small}
          placeholder="HH:MM:SS"
          required
        />
        <p>on the following days;</p>
      </Row>
      <Col>
        <FormikBitwiseCheckbox
          label="Monday"
          name={field('runOnWeekDays')}
          value={WeekDay.Monday}
        />
        <FormikBitwiseCheckbox
          label="Tuesday"
          name={field('runOnWeekDays')}
          value={WeekDay.Tuesday}
        />
        <FormikBitwiseCheckbox
          label="Wednesday"
          name={field('runOnWeekDays')}
          value={WeekDay.Wednesday}
        />
        <FormikBitwiseCheckbox
          label="Thursday"
          name={field('runOnWeekDays')}
          value={WeekDay.Thursday}
        />
        <FormikBitwiseCheckbox
          label="Friday"
          name={field('runOnWeekDays')}
          value={WeekDay.Friday}
        />
        <FormikBitwiseCheckbox
          label="Saturday"
          name={field('runOnWeekDays')}
          value={WeekDay.Saturday}
        />
        <FormikBitwiseCheckbox
          label="Sunday"
          name={field('runOnWeekDays')}
          value={WeekDay.Sunday}
        />
      </Col>
    </styled.Schedule>
  );
};
