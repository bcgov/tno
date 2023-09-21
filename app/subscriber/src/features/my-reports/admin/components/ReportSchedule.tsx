import {
  Col,
  FormikCheckbox,
  FormikStringEnumCheckbox,
  FormikTimeInput,
  Row,
  ScheduleWeekDayName,
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
        <FormikCheckbox name={`schedules.${index}.isEnabled`} label="Enabled" value={true} />
        <FormikCheckbox
          name={`schedules.${index}.settings.autoSend`}
          label="Auto Send"
          tooltip="Send emails to subscribers when report is run"
        />
        <FormikTimeInput
          name={`schedules.${index}.startAt`}
          label="Run At"
          width="7em"
          placeholder="HH:MM:SS"
        />
      </Col>
      <Col className="frm-in">
        <label>Run On</label>
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Monday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Monday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Tuesday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Tuesday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Wednesday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Wednesday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Thursday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Thursday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Friday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Friday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Saturday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Saturday}
        />
        <FormikStringEnumCheckbox<ScheduleWeekDayName>
          label="Sunday"
          name={`schedules.${index}.runOnWeekDays`}
          value={ScheduleWeekDayName.Sunday}
        />
      </Col>
    </Row>
  );
};
