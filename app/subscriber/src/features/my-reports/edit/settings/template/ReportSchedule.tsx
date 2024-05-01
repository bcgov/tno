import { Toggle } from 'components/form';
import { TimeInput } from 'components/form/timeinput';
import { getIn } from 'formik';
import moment from 'moment';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikStringEnumCheckbox,
  Row,
  ScheduleWeekDayName,
  selectWeekDays,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportScheduleProps {
  index: number;
  label: string;
}

export const ReportSchedule: React.FC<IReportScheduleProps> = ({ index, label }) => {
  const { values, setFieldValue, errors } = useReportEditContext();

  const schedule = values.events.length > index ? values.events[index] : undefined;

  return (
    <Row gap="1rem" className="schedule" nowrap>
      <Col gap="0.25rem" className="frm-in">
        <label>{label}</label>
        <FormikCheckbox name={`events.${index}.isEnabled`} label="Enabled" value={true} />
        <TimeInput
          label="Run at"
          placeholder="HH:MM:SS"
          value={schedule?.startAt ? schedule?.startAt : ''}
          width={FieldSize.Small}
          onChange={(value) => {
            setFieldValue(`events.${index}.startAt`, value);
          }}
          error={getIn(errors, `events.${index}.startAt`)}
        />
        <Toggle
          name="reset"
          label="Run again today"
          value={!schedule?.requestSentOn}
          disabled={!schedule?.startAt}
          options={[
            {
              label: 'No',
              value: false,
            },
            { label: 'Yes', value: true },
          ]}
          onChange={(value) => {
            if (value) setFieldValue(`events.${index}.requestSentOn`, undefined);
            else {
              const now = new Date();
              const dateStr = now.toISOString().split('T').shift();
              const sentOn = moment(`${dateStr} ${schedule?.startAt ?? '00:00'}`);
              setFieldValue(`events.${index}.requestSentOn`, sentOn.toISOString());
            }
          }}
        />

        <Col className="info">
          <p>
            A schedule that has already populated a report in a given day, will not attempt to do so
            again unless you allow it to 'Run again today'.
          </p>
        </Col>
      </Col>
      <Col className="frm-in">
        <label>Run On</label>
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Monday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Monday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Tuesday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Tuesday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Wednesday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Wednesday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Thursday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Thursday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Friday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Friday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Saturday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Saturday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Sunday"
          name={`events.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Sunday}
          onBeforeChange={(value) => selectWeekDays(value)}
        />
      </Col>
    </Row>
  );
};
