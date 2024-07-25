import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaEraser } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  formatDate,
  FormikCheckbox,
  FormikDatePicker,
  FormikStringEnumCheckbox,
  FormikText,
  FormikTimeInput,
  IReportModel,
  Row,
  ScheduleWeekDayName,
  selectWeekDays,
} from 'tno-core';

import * as styled from './styled';

export interface IReportScheduleProps {
  label: string;
  index: number;
}

export const ReportSchedule: React.FC<IReportScheduleProps> = ({ label, index }) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const event = values.events.length >= index ? values.events[index] : undefined;

  return (
    <styled.ReportSchedule>
      <Row gap="1rem">
        <Col>
          <h3>{label}</h3>
          <FormikCheckbox name={`events.${index}.isEnabled`} label="Enabled" value={true} />
          <FormikCheckbox
            name={`events.${index}.settings.autoSend`}
            label="Auto send"
            tooltip="Send emails to subscribers when report is run"
          />
        </Col>
        <Col>
          <FormikTimeInput
            name={`events.${index}.startAt`}
            label="Run at"
            width="7em"
            placeholder="HH:MM:SS"
          />
          <FormikDatePicker
            name={`events.${index}.runOn`}
            label="Start after"
            width="13em"
            showTimeInput
            showTimeSelect
            dateFormat="MM/dd/yyyy HH:mm:ss"
            value={
              values.events.length && values.events[index].runOn
                ? moment(values.events[index].runOn).format('MM/DD/yyyy HH:mm:ss')
                : undefined
            }
            onChange={(value) => {
              setFieldValue(`events.${index}.runOn`, value ? moment(value).toString() : undefined);
            }}
            isClearable
          />
        </Col>
        <Col className="frm-in">
          <label>Weekdays</label>
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
        <Col>
          <FormikText
            name={`events.${index}.requestSentOn`}
            label="Last Request Sent On"
            disabled
            value={formatDate(event?.requestSentOn ?? '', 'YYYY-MM-DD h:mm:ss a')}
          >
            <Button
              variant={ButtonVariant.danger}
              tooltip="Clear last request sent on"
              onClick={() => setFieldValue(`events.${index}.requestSentOn`, undefined)}
            >
              <FaEraser />
            </Button>
          </FormikText>
          <FormikText
            name={`events.${index}.lastRanOn`}
            label="Last Ran On"
            disabled
            value={formatDate(event?.lastRanOn ?? '', 'YYYY-MM-DD h:mm:ss a')}
          />
        </Col>
      </Row>
    </styled.ReportSchedule>
  );
};
