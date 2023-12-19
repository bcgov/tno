import {
  Col,
  FormikCheckbox,
  FormikStringEnumCheckbox,
  FormikTimeInput,
  Row,
  ScheduleWeekDayName,
  selectWeekDays,
} from 'tno-core';

export interface IReportScheduleProps {
  index: number;
  label: string;
}

export const ReportSchedule: React.FC<IReportScheduleProps> = ({ index, label }) => {
  return (
    <Row gap="1rem" className="schedule">
      <Col>
        <h3>{label}</h3>
        <FormikCheckbox name={`events.${index}.isEnabled`} label="Enabled" value={true} />
        <FormikCheckbox
          name={`events.${index}.settings.autoSend`}
          label="Auto Send"
          tooltip="Send emails to subscribers when report is run"
        />
        <FormikTimeInput
          name={`events.${index}.startAt`}
          label="Run At"
          width="7em"
          placeholder="HH:MM:SS"
        />
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
