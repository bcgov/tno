import { useFormikContext } from 'formik';
import { FaEraser } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTimeInput,
  IReportModel,
  Row,
  ScheduleWeekDayName,
} from 'tno-core';

import * as styled from './styled';

export interface IReportScheduleProps {
  label: string;
  index: number;
}

export const ReportSchedule: React.FC<IReportScheduleProps> = ({ label, index }) => {
  const { setFieldValue } = useFormikContext<IReportModel>();

  return (
    <styled.ReportSchedule>
      <Row gap="1rem">
        <Col>
          <h3>{label}</h3>
          <FormikCheckbox name={`schedules.${index}.isEnabled`} label="Enabled" value={true} />
          <FormikCheckbox
            name={`schedules.${index}.settings.autoSend`}
            label="Auto Send"
            tooltip="Send emails to subscribers when report is run"
          />
        </Col>
        <FormikTimeInput
          name={`schedules.${index}.startAt`}
          label="Run At"
          width="7em"
          placeholder="HH:MM:SS"
        />
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
        <Col>
          <FormikText
            name={`schedules.${index}.requestSentOn`}
            label="Last Request Sent On"
            disabled
          >
            <Button
              variant={ButtonVariant.danger}
              tooltip="Clear last request sent on"
              onClick={() => setFieldValue(`schedules.${index}.requestSentOn`, undefined)}
            >
              <FaEraser />
            </Button>
          </FormikText>
          <FormikText name={`schedules.${index}.lastRanOn`} label="Last Ran On" disabled />
        </Col>
      </Row>
    </styled.ReportSchedule>
  );
};
